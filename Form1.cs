using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;
using WindowsMotors.DataClasses;
using System.Data.Common;
using Windows.Media.Protection.PlayReady;
using WindowsMotors;
using System.Threading;
using Windows.Storage.Streams;
using Windows.Web.Http.Headers;
using Windows.Graphics.Holographic;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        MSPClient client;
        PositionMonitor monitor;
        RcMonitor rcMonitor;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbCommand.SelectedIndex = 0;
            client = new MSPClient(false);
            client.onData += Client_onData;

            monitor = new PositionMonitor(client);
            monitor.onUpdateUI += AutoAligner_onUpdateUI;

            rcMonitor = new RcMonitor(client);
        }

        private void RcMonitor_onDataUpdate(MspRcResponse rcResponse)
        {

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
                monitor.ProcessResponse(response);
                rcMonitor.ProcessResponse(response);
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
                    Aux5 = 1750,
                });
                monitor.Trottle = ushort.Parse(txtTrottle.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        private void checkOn_CheckedChanged(object sender, EventArgs e)
        {
            ushort data = checkOn.Checked ? (ushort)1 : (ushort)0;

            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = data, printDebug = checkPrint.Checked ? (ushort)1 : (ushort)0, delay = ushort.Parse(txtDelay.Text) });
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = 0, printDebug = checkPrint.Checked ? (ushort)1 : (ushort)0, delay = 1 });
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_RAW_RC, new MspSetRawRcRequest()
            {
                Aux1 = ushort.Parse(txtAux1.Text),
                Throttle = 1000,
                Roll = ushort.Parse(txtRoll.Text),
                Pitch = ushort.Parse(txtPitch.Text),
                Yaw = ushort.Parse(txtYaw.Text)
            });
            monitor.Trottle = ushort.Parse(txtTrottle.Text);
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
                monitor.Trottle = ushort.Parse(txtTrottle.Text);
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

        private void checkBoxAlign_CheckedChanged(object sender, EventArgs e)
        {
            if (checkMonitor.Checked)
            {
                monitor.Monitor();
            }
            else
            {
                monitor.Stop();
            }

        }

        private void AutoAligner_onUpdateUI(MspDebugDataResponse response)
        {
            this.Invoke(new Action(() =>
            {
                if (response == null)
                {
                    return;
                }
                labelPitch.Text = response.Pitch.ToString();
                labelRoll.Text = response.Roll.ToString();
                labelYaw.Text = response.Yaw.ToString();
                labelSetpointRoll.Text = response.pidSetpointRoll.ToString();
                labelSetpointPitch.Text = response.pidSetpointPitch.ToString();
                labelSetpointYaw.Text = response.pidSetpointYaw.ToString();
                labelSumRoll.Text = response.pidSumRoll.ToString();
                labelSumPitch.Text = response.pidSumPitch.ToString();
                labelSumYaw.Text = response.pidSumYaw.ToString();
            }));
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            var view = new WindowsMotors.OpenCVView(
                onnxPath: "config_files/yolov5s.onnx",
                classesPath: "config_files/classes.txt",
                cameraIndex: 1,
                gstreamerPipeline: null,
                showWindow: true);

            view.OnDetect += View_OnDetect;

            view.Start();
        }

        private static short frameID = 0;

        private void View_OnDetect(OpenCvSharp.Mat img, List<OpenCVView.Detection> detections, List<string> classNames)
        {
            foreach (var detection in detections)
            {
                if (detection.ClassId == 41)
                {
                    if (frameID == short.MaxValue)
                    {
                        frameID = 0;
                    }
                    frameID ++;
                    int deltaYaw = img.Width / 2 - (detection.Box.Left + detection.Box.Width / 2);
                    int deltaPitch = img.Height / 2 - (detection.Box.Top + detection.Box.Height / 2);

                    this.Invoke(new Action(() =>
                    {
                        Text = $"$Delta X: {deltaYaw}  frameID {frameID}";
                    }));

                    client.SendCommand(MSPClient.MSPCommand.MSP_SET_AUTOPILOT_DATA, new MspSetAutopilotDataRequest()
                    {
                        DeltaYaw = (short)deltaYaw,
                        DeltaPitch = (short)deltaPitch,
                        InitialYaw = 100,
                        InitialPitch = 100,
                        FrameID = frameID
                        
                    });
                    Thread.Sleep(80);

                }
            }

        }
    }
}
