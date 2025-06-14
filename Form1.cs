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

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        MSPClient client;
        Thread safeThread;

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
            MSPResponse response = MSPClient.ParseResponse(rawData);
           

            if (safeThread != null)
            {
                if (response is MspAttitudeResponse)
                {
                    MspAttitudeResponse attitudeResponse = (MspAttitudeResponse)response;
                    this.Invoke(new Action(() =>
                    {
                        txtAttitude.Text = attitudeResponse.ToString();
                    }));

                    if (Math.Abs(attitudeResponse.Roll) > 20 || Math.Abs(attitudeResponse.Pitch) > 20)
                    {
                        txtAttitude.ForeColor = Color.Red;
                    }
                    else
                    {
                        txtAttitude.ForeColor = Color.Black;
                    }
                }
                else if (response is MspAltitudeResponse)
                {
                    MspAltitudeResponse altitudeResponse = (MspAltitudeResponse)response;
                    this.Invoke(new Action(() =>
                    {
                        txtAltitude.Text = altitudeResponse.ToString();
                    }));
                    
                }
                else if (response is MspMotorResponse)
                {
                    MspMotorResponse motorResponse = (MspMotorResponse)response;
                    this.Invoke(new Action(() =>
                    {
                        txtMotor.Text = motorResponse.ToString();
                    }));

                }
            }
            else
            {
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
            MSPClient.MSPCommand mspCommand = checkBoxAuto.Checked ? MSPClient.MSPCommand.MSP_SET_AUTO_RC : MSPClient.MSPCommand.MSP_SET_RAW_RC;
            try
            {
                client.SendCommand(mspCommand, new MspSetRawRcRequest()
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
 

        private void checkOn_CheckedChanged(object sender, EventArgs e)
        {
            ushort data = checkOn.Checked ? (ushort)1 : (ushort)0;

            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = data, printDebug = checkPrint.Checked ? (ushort)1 : (ushort)0, delay =  ushort.Parse(txtDelay.Text) });
        }
 
     

        private void ProcessSafeMode()
        {
            try
            {
                while (true)
                {
                    client.SendCommand(MSPClient.MSPCommand.MSP_ALTITUDE, new byte[] { });
                    Thread.Sleep(10);
                    client.SendCommand(MSPClient.MSPCommand.MSP_ATTITUDE, new byte[] { });
                    Thread.Sleep(10);
                    client.SendCommand(MSPClient.MSPCommand.MSP_MOTOR, new byte[] { });
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }   

           
        }

        private void checkSafeMode_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSafeMode.Checked)
            {
                safeThread = new Thread(ProcessSafeMode);
                safeThread.Start();
            }
            else
            {
                safeThread.Abort();
                safeThread = null;
            }
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
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            short stepTrottle = 5;
            short stepMove = 20;
            Text = e.KeyValue.ToString();
            switch (e.KeyCode)
            {
                case Keys.Up:
                    txtPitch.Text = (Convert.ToUInt16(txtPitch.Text)+ stepMove).ToString();
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
            MSPClient.MSPCommand mspCommand = checkBoxAuto.Checked ? MSPClient.MSPCommand.MSP_SET_AUTO_RC : MSPClient.MSPCommand.MSP_SET_RAW_RC;

            try
            {
                client.SendCommand(mspCommand, new MspSetRawRcRequest()
                {
                    Aux1 = ushort.Parse(txtAux1.Text),
                    Throttle = ushort.Parse(txtTrottle.Text),
                    Roll = ushort.Parse(txtRoll.Text),
                    Pitch = ushort.Parse(txtPitch.Text),
                    Yaw = ushort.Parse(txtYaw.Text)
                });
            }
            catch (Exception ex) {
                Console.WriteLine( ex.Message );
            }   

           



        }

        private void checkBoxAuto_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
