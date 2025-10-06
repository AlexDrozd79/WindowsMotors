using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Tracking;
using Windows.Graphics;
using WindowsMotors;
using static OpenCvSharp.Stitcher;
 

namespace WindowsFormsApp1
{

    public struct ObjectInfo
    {
        public Point2f Center;
        public Rect Bbox;
        public bool Detected;
        public float Distance;
        public double Confidence;
    }
    public class VisionTracker : IDisposable
    {
        private VideoCapture _cap = new VideoCapture();
        private TrackerCSRT _tracker;
        private bool _trackerInit;

        private readonly int _targetArea = 8000;
        private readonly float _targetDist = 2.0f;

        public delegate void OnDetectDelegate(Mat img, ObjectInfo info);
        public event OnDetectDelegate OnDetect;

        /// Remove later
        /// </summary>
        // HSV (OpenCV): H 0..179, S/V 0..255
        static readonly Scalar HSV_RED_LO_A = new Scalar(0, 70, 40);   // навколо нуля
        static readonly Scalar HSV_RED_HI_A = new Scalar(8, 255, 255);

        static readonly Scalar HSV_RED_LO_B = new Scalar(170, 70, 40);   // wrap біля 179
        static readonly Scalar HSV_RED_HI_B = new Scalar(179, 255, 255);

        static readonly Scalar HSV_ORANGE_LO = new Scalar(8, 60, 50);   // широкий оранжевий
        static readonly Scalar HSV_ORANGE_HI = new Scalar(35, 255, 255);

        // YCrCb пороги (червоність)
        static readonly Scalar YCRCB_LO = new Scalar(0, 145, 60); // Y,Cr,Cb
        static readonly Scalar YCRCB_HI = new Scalar(255, 255, 180);



        public bool InitCam(int camId = 0)
        {
            _cap = new VideoCapture(camId, VideoCaptureAPIs.DSHOW);
            if (!_cap.IsOpened()) return false;
            return true;
        }

        public bool Grab(Mat frame) => _cap.Read(frame);



        public void ResetTracker()
        {
            if (_tracker != null) _tracker.Dispose();
            _tracker = null;
            _trackerInit = false;
        }

        public bool InitTracker(Mat frame, Rect bbox)
        {
            ResetTracker();
            _tracker = TrackerCSRT.Create();
            _tracker.Init(frame, bbox);
            _trackerInit = true;
            return _trackerInit;
        }

        public ObjectInfo Track(Mat frame)
        {
            var info = new ObjectInfo { Detected = false };
            if (!_trackerInit || _tracker == null) return info;

            Rect bb = default(Rect);
            if (_tracker.Update(frame, ref bb))
            {
                Rect r = new Rect((int)bb.X, (int)bb.Y, (int)bb.Width, (int)bb.Height);
                info.Bbox = r;
                info.Center = new Point2f(r.X + r.Width * 0.5f, r.Y + r.Height * 0.5f);
                info.Detected = true;
                info.Confidence = 0.8;
                info.Distance = EstimateDistance(r);
            }
            else
            {
                _trackerInit = false;
            }
            return info;
        }

        private ObjectInfo DetectPumpkinRobust(Mat frame)
        {
            var info = new ObjectInfo { Detected = false };

            using (var den = new Mat())
            {
                Cv2.GaussianBlur(frame, den, new Size(5, 5), 0);

                using (var hsv = new Mat())
                using (var mRedA = new Mat())
                using (var mRedB = new Mat())
                using (var mOrange = new Mat())
                using (var mHSV = new Mat())
                using (var ycrcb = new Mat())
                using (var mYCC = new Mat())
                using (var mask = new Mat())
                {
                    // HSV
                    Cv2.CvtColor(den, hsv, ColorConversionCodes.BGR2HSV);
                    Cv2.InRange(hsv, HSV_RED_LO_A, HSV_RED_HI_A, mRedA);
                    Cv2.InRange(hsv, HSV_RED_LO_B, HSV_RED_HI_B, mRedB);
                    Cv2.InRange(hsv, HSV_ORANGE_LO, HSV_ORANGE_HI, mOrange);

                    Cv2.BitwiseOr(mRedA, mRedB, mHSV);
                    Cv2.BitwiseOr(mHSV, mOrange, mHSV);

                    // YCrCb
                    Cv2.CvtColor(den, ycrcb, ColorConversionCodes.BGR2YCrCb);
                    Cv2.InRange(ycrcb, YCRCB_LO, YCRCB_HI, mYCC);

                    // Перетин + морфологія
                    Cv2.BitwiseAnd(mHSV, mYCC, mask);

                    using (var k = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7)))
                    {
                        Cv2.MorphologyEx(mask, mask, MorphTypes.Open, k);
                        Cv2.MorphologyEx(mask, mask, MorphTypes.Close, k);
                    }

                    // Контури
                    Point[][] contours;
                    HierarchyIndex[] _;
                    Cv2.FindContours(mask, out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    if (contours == null || contours.Length == 0) return info;

                    double bestScore = 0;
                    int best = -1;
                    for (int i = 0; i < contours.Length; i++)
                    {
                        double area = Cv2.ContourArea(contours[i]);
                        if (area < 120) continue;

                        double peri = Math.Max(1e-3, Cv2.ArcLength(contours[i], true));
                        double circ = 4.0 * Math.PI * area / (peri * peri);
                        double score = area * (0.5 + 0.5 * Math.Min(1.0, circ));

                        if (score > bestScore) { bestScore = score; best = i; }
                    }

                    if (best >= 0)
                    {
                        var r = Cv2.BoundingRect(contours[best]);
                        info.Bbox = r;
                        info.Center = new Point2f((float)(r.X + r.Width * 0.5), (float)(r.Y + r.Height * 0.5));
                        info.Detected = true;
                        info.Confidence = Math.Min(1.0, bestScore / 9000.0);
                        info.Distance = EstimateDistance(r);
                    }
                }
            }

            return info;
        }

        public void Draw(Mat frame, ObjectInfo obj)
        {
            int cx = frame.Cols / 2, cy = frame.Rows / 2;
            Cv2.Line(frame, new Point(cx - 20, cy), new Point(cx + 20, cy), new Scalar(0, 255, 255), 2);
            Cv2.Line(frame, new Point(cx, cy - 20), new Point(cx, cy + 20), new Scalar(0, 255, 255), 2);

            if (obj.Detected)
            {
                Cv2.Rectangle(frame, obj.Bbox, new Scalar(0, 255, 0), 2);
                Cv2.Circle(frame, (Point)obj.Center, 4, new Scalar(255, 0, 0), -1);
                Cv2.Line(frame, new Point(cx, cy), (Point)obj.Center, new Scalar(255, 255, 0), 1);


            }
            else
            {
                Cv2.PutText(frame, "SEARCHING...", new Point(10, 30),
                    HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 0, 255), 2);
            }

            Cv2.PutText(frame, "Mode: color", new Point(10, frame.Rows - 20),
                HersheyFonts.HersheySimplex, 0.6, new Scalar(255, 255, 255), 1);
        }

        // ---------- utils ----------



        private float EstimateDistance(Rect b)
        {
            float area = Math.Max(1, b.Width * b.Height);
            return _targetDist * (float)Math.Sqrt(_targetArea / area);
        }



        public void Dispose()
        {
            try
            {
                ResetTracker();
                if (_cap != null)
                {
                    _cap.Release();
                    _cap.Dispose();
                }


            }
            catch { /* ignore */ }
        }
        public void Start()
        {
            using (var vt = new VisionTracker())
            {
                if (!vt.InitCam(1)) { Console.WriteLine("Camera open failed"); return; }

                string outFile = "C:\\My\\WindowsMotors\\output.avi";
                int fourcc = FourCC.XVID;          // або FourCC.MJPG
                double fps = 30.0;
                Size frameSize = new Size(1280, 720);
                var writer = new VideoWriter(outFile, fourcc, fps, frameSize, true);
                if (!writer.IsOpened())
                {
                    Console.WriteLine("❌ Cannot open video writer!");
                    return;
                }
                Console.WriteLine($"🎥 Recording to {outFile}");

                bool tracking = false;

                var win = new Window("Drone Tracker C#");
                var frame = new Mat();

                ObjectInfo obj = default(ObjectInfo);

                while (true)
                {
                    if (!vt.Grab(frame) || frame.Empty()) { MessageBox.Show("Frame error"); break; }

                    int key = Cv2.WaitKey(1) & 0xFF;
                    if (key == 'q') break;
                    if (key == '1') { tracking = !tracking; }

                    if (tracking)
                    {
                        // 1) спроба трекінгу
                        obj = vt.Track(frame);

                        // 2) втрата — робимо детект + ініт трекера
                        if (!obj.Detected)
                        {
                            obj = vt.DetectPumpkinRobust(frame);
                            if (obj.Detected)
                            {
                                vt.InitTracker(frame, obj.Bbox);
                            }
                        }

                    }
                    else
                    {
                        obj.Detected = false;
                    }

                    OnDetect?.Invoke(frame.Clone(), obj);
                    

                    vt.Draw(frame, obj);
                    win.ShowImage(frame);
                    writer.Write(frame);
                }
                writer.Release();
                Cv2.DestroyAllWindows();

            }
        }
    }
}
