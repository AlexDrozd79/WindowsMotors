using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Graphics.Holographic;
using Windows.Media.Protection.PlayReady;
using Windows.Storage.Streams;
using Windows.Web.Http.Headers;
using WindowsMotors;
using WindowsMotors.DataClasses;
using static WindowsMotors.OpenCVView;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        MSPClient client;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbCommand.SelectedIndex = 0;
            client = new MSPClient(false);
            client.onData += Client_onData;



        }

        private void Client_onData(MSPClient sender, byte[] rawData)
        {
            if (client.isCLIMode || (rawData.Length == 4 && Encoding.ASCII.GetString(rawData) == "exit"))
            {
                string strOutput = Encoding.ASCII.GetString(rawData);
                this.Invoke(new Action(() =>
                {
                    txtOutput.AppendText(strOutput + Environment.NewLine);
                }));
            }
            else
            {
                MSPResponse response = MSPClient.ParseResponse(rawData);
                this.Invoke(new Action(() =>
             {
                 txtOutput.AppendText(response.ToString() + Environment.NewLine);
             }));

            }

        }


        private void cmdCommand_Click(object sender, EventArgs e)
        {
            int command = int.Parse(cmbCommand.Text.Split('(')[1].Replace(")", ""));
            switch ((MSPClient.MSPCommand)command)
            {
                case MSPClient.MSPCommand.MSP_ARMING_CONFIG:
                    client.SendCommand(MSPClient.MSPCommand.MSP_ARMING_CONFIG, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_STATUS:
                    client.SendCommand(MSPClient.MSPCommand.MSP_STATUS, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_RAW_IMU:
                    client.SendCommand(MSPClient.MSPCommand.MSP_RAW_IMU, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_SERVO:
                    client.SendCommand(MSPClient.MSPCommand.MSP_SERVO, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_MOTOR:
                    client.SendCommand(MSPClient.MSPCommand.MSP_MOTOR, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_RC:
                    client.SendCommand(MSPClient.MSPCommand.MSP_RC, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_ATTITUDE:
                    client.SendCommand(MSPClient.MSPCommand.MSP_ATTITUDE, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_ALTITUDE:
                    client.SendCommand(MSPClient.MSPCommand.MSP_ALTITUDE, new byte[] { });
                    break;
                case MSPClient.MSPCommand.MSP_DEBUG_DATA:
                    client.SendCommand(MSPClient.MSPCommand.MSP_DEBUG_DATA, new byte[] { });
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
        }

        private void buttonRawData_Click(object sender, EventArgs e)
        {
            client.SendCommand(textBoxRawCommad.Text);
        }


        private void trackBarMotorsSpeed_Scroll(object sender, EventArgs e)
        {
            ushort speed = (ushort)trackBarMotorsSpeed.Value;

            client.SendCommand(MSPClient.MSPCommand.MSP_SET_MOTOR, new MotorSpeedRequest() { motor1Speed = speed, motor2Speed = speed, motor3Speed = speed, motor4Speed = speed });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
                {
                    Aux1 = ushort.Parse(txtAux1.Text),
                    Throttle = ushort.Parse(txtTrottle.Text),
                    Roll = ushort.Parse(txtRoll.Text),
                    Pitch = ushort.Parse(txtPitch.Text),
                    Yaw = ushort.Parse(txtYaw.Text),
                    Aux4 = 1550,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void checkOn_CheckedChanged(object sender, EventArgs e)
        {
            ushort data = checkOn.Checked ? (ushort)1 : (ushort)0;
            ushort justThrottle = checkJustThrottle.Checked ? (ushort)1 : (ushort)0;
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
            {
                Aux1 = 1600,
                Throttle = 1000,
                Roll = 1500,
                Pitch = 1500,
                Yaw = 1500,
                Aux5 = 1750,
            });
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = data, justThrottle = justThrottle, delay = ushort.Parse(txtDelay.Text) });
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = 0, delay = 1 });
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
            {
                Aux1 = ushort.Parse(txtAux1.Text),
                Throttle = 1000,
                Roll = ushort.Parse(txtRoll.Text),
                Pitch = ushort.Parse(txtPitch.Text),
                Yaw = ushort.Parse(txtYaw.Text)
            });

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            short stepTrottle = 5;
            short stepMove = 20;
            Text = e.KeyValue.ToString();
            switch (e.KeyCode)
            {
                case Keys.Up:
                    txtPitch.Text = (Convert.ToUInt16(txtPitch.Text) + stepMove).ToString();
                    break;
                case Keys.Down:
                    txtPitch.Text = (Convert.ToUInt16(txtPitch.Text) - stepMove).ToString();
                    break;
                case Keys.Left:
                    txtRoll.Text = (Convert.ToUInt16(txtRoll.Text) - stepMove).ToString();
                    break;
                case Keys.Right:
                    txtRoll.Text = (Convert.ToUInt16(txtRoll.Text) + stepMove).ToString();
                    break;
                case Keys.S:
                    txtTrottle.Text = (Convert.ToUInt16(txtTrottle.Text) + stepTrottle).ToString();
                    break;
                case Keys.A:
                    txtTrottle.Text = (Convert.ToUInt16(txtTrottle.Text) - 15).ToString();
                    break;
            }

            try
            {
                client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
                {
                    Aux1 = ushort.Parse(txtAux1.Text),
                    Throttle = ushort.Parse(txtTrottle.Text),
                    Roll = ushort.Parse(txtRoll.Text),
                    Pitch = ushort.Parse(txtPitch.Text),
                    Yaw = ushort.Parse(txtYaw.Text)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private void checkBoxCLI_CheckedChanged(object sender, EventArgs e)
        {
            client.isCLIMode = checkBoxCLI.Checked;
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            bool useModel = false;
            if (useModel)
            {
                var view = new WindowsMotors.OpenCVView(
                    onnxPath: "config_files/yolov5s.onnx",
                    classesPath: "config_files/classes.txt",
                    cameraIndex: 1,
                    gstreamerPipeline: null,
                    showWindow: true,
                    justDisplay: true);

                view.OnDetect += View_OnDetect;

                view.Start();
            }
            else
            {
                VisionTracker tracker = new VisionTracker();
                tracker.OnDetect += Tracker_OnDetect;
                tracker.Start();
            }
        }

        private static short frameID = 0;

        private void Tracker_OnDetect(OpenCvSharp.Mat img, ObjectInfo info)
        {
            if (info.Detected)
            {
                if (frameID == short.MaxValue)
                {
                    frameID = 0;
                }
                frameID++;
                int deltaYaw = img.Width / 2  - (info.Bbox.Left + info.Bbox.Width / 2);
                int deltaPitch = (info.Bbox.Top + info.Bbox.Height / 2) - img.Height / 2;
                int deltaRoll = -(img.Width / 2 - (info.Bbox.Left + info.Bbox.Width / 2));

                client.SendCommand(MSPClient.MSPCommand.MSP_SET_AUTOPILOT_DATA, new MspSetAutopilotDataRequest()
                {
                    DeltaYaw = (short)deltaYaw,
                    DeltaPitch = (short)deltaPitch,
                    DeltaRoll = (short)deltaRoll,
                    InitialYaw = 10,
                    InitialPitch = 0,
                    InitialRoll = 20,
                    FrameID = frameID,
                    ModeID = 1

                });
                Thread.Sleep(80);
            }
        }

        private void View_OnDetect(OpenCvSharp.Mat img, List<OpenCVView.Detection> detections, List<string> classNames)
        {
            bool isDetected = false;
            foreach (var detection in detections)
            {
                if (detection.ClassId == 74) //74 - clock, 41- cup
                {
                    if (frameID == short.MaxValue)
                    {
                        frameID = 0;
                    }
                    frameID++;
                    int deltaYaw = img.Width / 2 - (detection.Box.Left + detection.Box.Width / 2);
                    int deltaPitch = (detection.Box.Top + detection.Box.Height / 2) - img.Height / 2 - 160;

                    this.Invoke(new Action(() =>
                    {
                        Text = $"$Delta X: {deltaYaw}  frameID {frameID}";
                    }));

                    client.SendCommand(MSPClient.MSPCommand.MSP_SET_AUTOPILOT_DATA, new MspSetAutopilotDataRequest()
                    {
                        DeltaYaw = (short)deltaYaw,
                        DeltaPitch = (short)deltaPitch,
                        InitialYaw = 10,
                        InitialPitch = 100,
                        FrameID = frameID,
                        ModeID = 1

                    });

                    client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
                    {
                        Aux1 = ushort.Parse(txtAux1.Text),
                        Throttle = ushort.Parse(txtTrottle.Text),//(ushort)PositionAdjuster.adjustThrottle((short)deltaPitch),
                        Roll = ushort.Parse(txtRoll.Text),
                        Pitch = ushort.Parse(txtPitch.Text),
                        Yaw = checkJustThrottle.Checked ? (ushort)1500 : (ushort)PositionAdjuster.adjustYaw((short)deltaYaw, 100),
                        Aux5 = 1500,
                    });

                    isDetected = true;
                    Thread.Sleep(80);

                }
                if (!isDetected)
                {

                }
            }

        }

    }
}
