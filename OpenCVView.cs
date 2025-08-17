// OpenCVView.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace WindowsMotors
{
    public sealed class OpenCVView : IDisposable
    {
        private const float INPUT_WIDTH = 640f;
        private const float INPUT_HEIGHT = 640f;
        private const float SCORE_THRESHOLD = 0.2f;
        private const float NMS_THRESHOLD = 0.4f;
        private const float CONFIDENCE_THRESHOLD = 0.4f;

        private static readonly Scalar[] BoxColors = new Scalar[]
        {
            new Scalar(255,255,0),
            new Scalar(0,255,0),
            new Scalar(0,255,255),
            new Scalar(255,0,0),
            new Scalar(255,0,255),
            new Scalar(0,128,255)
        };

        private readonly string _onnxPath;
        private readonly string _classesPath;
        private readonly string _gstreamerPipeline; // null якщо не використовуємо
        private readonly int _cameraIndex;          // якщо pipeline == null
        private readonly bool _showWindow;

        private VideoCapture _capture;
        private Net _net;
        private List<string> _classNames;

        private Thread _worker;
        private volatile bool _running;
        private readonly object _lock = new object();

        // кадр після рендеру (як і було)
        public event Action<Mat> FrameReady;

        // ► делегат + івент для детекцій
        public delegate void OnDetectDelegate(Mat img, List<Detection> detections, List<string> classNames);
        public event OnDetectDelegate OnDetect;

        public OpenCVView(string onnxPath,
                          string classesPath,
                          int cameraIndex = 0,
                          string gstreamerPipeline = null,
                          bool showWindow = true)
        {
            _onnxPath = onnxPath;
            _classesPath = classesPath;
            _cameraIndex = cameraIndex;
            _gstreamerPipeline = gstreamerPipeline;
            _showWindow = showWindow;
        }

        public bool Start()
        {
            try
            {
                if (!File.Exists(_onnxPath))
                    throw new FileNotFoundException("ONNX model not found", _onnxPath);
                if (!File.Exists(_classesPath))
                    throw new FileNotFoundException("classes.txt not found", _classesPath);

                _classNames = LoadClassList(_classesPath);
                _net = LoadNet(_onnxPath);

                _capture = OpenVideoSource(_gstreamerPipeline, _cameraIndex);
                if (_capture == null || !_capture.IsOpened())
                    throw new InvalidOperationException("Cannot open video source.");

                _running = true;
                _worker = new Thread(Loop) { IsBackground = true, Name = "OpenCVViewLoop" };
                _worker.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[OpenCVView.Start] " + ex);
                Stop();
                return false;
            }
        }

        public void Stop()
        {
            _running = false;
            try
            {
                if (_worker != null && _worker.IsAlive)
                    _worker.Join(500);
            }
            catch { /* ignore */ }

            lock (_lock)
            {
                if (_capture != null) { _capture.Release(); _capture.Dispose(); _capture = null; }
                if (_net != null) { _net.Dispose(); _net = null; }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void Loop()
        {
            Mat frame = null;
            Mat display = null;
            Window window = null;

            try
            {
                if (_showWindow)
                    window = new Window("OpenCVView");

                frame = new Mat();
                display = new Mat();

                int frameCount = 0;
                int totalFrames = 0;
                double fps = -1;
                var tick = DateTime.UtcNow;

                while (_running)
                {
                    bool ok;
                    lock (_lock)
                    {
                        ok = _capture != null && _capture.Read(frame);
                    }
                    if (!ok || frame.Empty())
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    frame.CopyTo(display);

                    List<Detection> detections = Detect(display, _net, _classNames);
                    DrawDetections(display, detections, _classNames);

                    // ► виклик івента після детекції
                    if (OnDetect != null)
                    {
                        Mat detImg = display.Clone(); // захист від життєвого циклу матриці
                        try
                        {
                            OnDetect(detImg, new List<Detection>(detections), _classNames);
                        }
                        finally { detImg.Dispose(); }
                    }

                    frameCount++; totalFrames++;
                    var elapsed = (DateTime.UtcNow - tick).TotalMilliseconds;
                    if (elapsed >= 1000.0)
                    {
                        fps = frameCount * 1000.0 / elapsed;
                        frameCount = 0;
                        tick = DateTime.UtcNow;
                    }
                    if (fps > 0)
                    {
                        Cv2.PutText(display, "FPS: " + fps.ToString("F2"),
                            new Point(10, 25), HersheyFonts.HersheySimplex, 0.8,
                            new Scalar(0, 0, 255), 2);
                    }

                    if (FrameReady != null)
                    {
                        Mat delivered = display.Clone();
                        try { FrameReady(delivered); }
                        finally { delivered.Dispose(); }
                    }

                    if (_showWindow && window != null)
                    {
                        window.ShowImage(display);
                        int k = Cv2.WaitKey(1);
                        if (k == 27) // ESC
                        {
                            _running = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[OpenCVView.Loop] " + ex);
            }
            finally
            {
                if (window != null) window.Dispose();
                if (display != null) display.Dispose();
                if (frame != null) frame.Dispose();
            }
        }

        private static List<string> LoadClassList(string path)
        {
            var list = new List<string>();
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var s = lines[i];
                if (!string.IsNullOrWhiteSpace(s))
                    list.Add(s.Trim());
            }
            return list;
        }

        private static Net LoadNet(string onnxPath)
        {
            Net net = CvDnn.ReadNet(onnxPath);

            try
            {
                net.SetPreferableBackend(Backend.CUDA);
                net.SetPreferableTarget(Target.CUDA_FP16);
            }
            catch
            {
                net.SetPreferableBackend(Backend.OPENCV);
                net.SetPreferableTarget(Target.CPU);
            }

            return net;
        }

        private static VideoCapture OpenVideoSource(string pipeline, int cameraIndex)
        {
            VideoCapture cap = null;

            if (!string.IsNullOrEmpty(pipeline))
            {
                cap = new VideoCapture(pipeline, VideoCaptureAPIs.GSTREAMER);
            }
            else
            {
                cap = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
                if (!cap.IsOpened())
                {
                    cap.Dispose();
                    cap = new VideoCapture(cameraIndex, VideoCaptureAPIs.MSMF);
                }
            }

            return cap;
        }

        // зробив public, щоб тип був доступний підписникам івента
        public struct Detection
        {
            public int ClassId;
            public float Confidence;
            public Rect Box;
        }

        private static Mat Letterbox(Mat src)
        {
            int w = src.Cols;
            int h = src.Rows;
            int m = Math.Max(w, h);
            Mat dst = new Mat(new Size(m, m), MatType.CV_8UC3, Scalar.All(0));
            src.CopyTo(new Mat(dst, new Rect(0, 0, w, h)));
            return dst;
        }

        private static List<Detection> Detect(Mat frameBgr, Net net, List<string> classNames)
        {
            List<Detection> output = new List<Detection>();

            Mat square = null;
            Mat blob = null;
            Mat outMat = null;
            Mat logits = null;

            try
            {
                square = Letterbox(frameBgr);
                blob = CvDnn.BlobFromImage(
                    square, 1.0 / 255.0,
                    new Size((int)INPUT_WIDTH, (int)INPUT_HEIGHT),
                    new Scalar(), true, false);

                net.SetInput(blob);

                outMat = net.Forward();

                int rows = outMat.Size(1);
                int dims = outMat.Size(2);

                logits = outMat.Reshape(1, rows);
                var m = logits.GetGenericIndexer<float>();

                float xFactor = (float)square.Cols / INPUT_WIDTH;
                float yFactor = (float)square.Rows / INPUT_HEIGHT;

                List<int> classIds = new List<int>();
                List<float> confidences = new List<float>();
                List<Rect> boxes = new List<Rect>();

                for (int i = 0; i < rows; i++)
                {
                    float conf = m[i, 4];
                    if (conf < CONFIDENCE_THRESHOLD) continue;

                    int bestClass = -1;
                    float bestScore = 0f;

                    for (int c = 5; c < dims; c++)
                    {
                        float s = m[i, c];
                        if (s > bestScore)
                        {
                            bestScore = s;
                            bestClass = c - 5;
                        }
                    }

                    if (bestScore < SCORE_THRESHOLD) continue;

                    float cx = m[i, 0];
                    float cy = m[i, 1];
                    float w = m[i, 2];
                    float h = m[i, 3];

                    int left = (int)((cx - 0.5f * w) * xFactor);
                    int top = (int)((cy - 0.5f * h) * yFactor);
                    int width = (int)(w * xFactor);
                    int height = (int)(h * yFactor);

                    classIds.Add(bestClass);
                    confidences.Add(conf);
                    boxes.Add(new Rect(left, top, width, height));
                }

                // власний NMS
                List<int> kept = Nms(boxes, confidences, SCORE_THRESHOLD, NMS_THRESHOLD);

                for (int k = 0; k < kept.Count; k++)
                {
                    int idx = kept[k];
                    Detection det = new Detection();
                    det.ClassId = classIds[idx];
                    det.Confidence = confidences[idx];
                    det.Box = boxes[idx];
                    output.Add(det);
                }
            }
            finally
            {
                if (logits != null) logits.Dispose();
                if (outMat != null) outMat.Dispose();
                if (blob != null) blob.Dispose();
                if (square != null) square.Dispose();
            }

            return output;
        }

        private static void DrawDetections(Mat img, List<Detection> detections, List<string> classNames)
        {
            Cv2.Rectangle(img, new Rect(new Point(img.Width/2-2, img.Height/2-2), new Size(4, 4)), new Scalar(34, 139, 34), -1);
            for (int i = 0; i < detections.Count; i++)
            {
                Detection d = detections[i];
                Scalar color = BoxColors[d.ClassId % BoxColors.Length];
                Cv2.Rectangle(img, d.Box, color, 2);

                string label = (d.ClassId >= 0 && d.ClassId < classNames.Count)
                    ? classNames[d.ClassId]
                    : ("id:" + d.ClassId);

                string text = string.Format("{0} {1:0.00}", label, d.Confidence);
                int baseline = 0;
                Size tsize = Cv2.GetTextSize(text, HersheyFonts.HersheySimplex, 0.5, 1, out baseline);
                int x = Math.Max(d.Box.X, 0);
                int y = Math.Max(d.Box.Y - tsize.Height - 4, 0);

                Cv2.Rectangle(img, new Rect(new Point(x, y), new Size(tsize.Width + 6, tsize.Height + 6)), color, -1);
                Cv2.PutText(img, text, new Point(x + 3, y + tsize.Height + 1),
                    HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 1);
            }
        }

        // ------------------ NMS + IoU ------------------
        private static List<int> Nms(IList<Rect> boxes, IList<float> scores, float scoreThresh, float nmsThresh)
        {
            var idxs = new List<int>();
            if (boxes == null || scores == null || boxes.Count == 0 || scores.Count != boxes.Count)
                return idxs;

            var order = Enumerable.Range(0, boxes.Count)
                                  .Where(i => scores[i] >= scoreThresh)
                                  .OrderByDescending(i => scores[i])
                                  .ToList();

            var suppressed = new bool[boxes.Count];

            for (int i = 0; i < order.Count; i++)
            {
                int idx = order[i];
                if (suppressed[idx]) continue;

                idxs.Add(idx);

                for (int j = i + 1; j < order.Count; j++)
                {
                    int idx2 = order[j];
                    if (suppressed[idx2]) continue;

                    float iou = IoU(boxes[idx], boxes[idx2]);
                    if (iou > nmsThresh)
                        suppressed[idx2] = true;
                }
            }
            return idxs;
        }

        private static float IoU(Rect a, Rect b)
        {
            int x1 = Math.Max(a.X, b.X);
            int y1 = Math.Max(a.Y, b.Y);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            int w = Math.Max(0, x2 - x1);
            int h = Math.Max(0, y2 - y1);

            int inter = w * h;
            int areaA = a.Width * a.Height;
            int areaB = b.Width * b.Height;
            int uni = areaA + areaB - inter;

            if (uni <= 0) return 0f;
            return (float)inter / (float)uni;
        }
        // -----------------------------------------------
    }
}
