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
			this.Invoke(new Action(() =>
			{
				txtOutput.AppendText(response.ToString() + Environment.NewLine);
			}));

			if (safeThread != null)
			{
				if (response is MspAttitudeResponse)
				{
                    MspAttitudeResponse attitudeResponse = (MspAttitudeResponse)response;
					if (Math.Abs(attitudeResponse.Roll) > 20 || Math.Abs(attitudeResponse.Pitch) > 20)
					{

					}

                }
			}

		}

		private async void button1_Click(object sender, EventArgs e)
        {
            var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(57964256891010);

            if (bleDevice != null)
            {
                MessageBox.Show($"Connected to {bleDevice.Name}");

                // Example: Enumerate the GATT services of the connected device
                var result = await bleDevice.GetGattServicesAsync();

                if (result.Status == GattCommunicationStatus.Success)
                {
                    foreach (var service in result.Services)
                    {
                        System.Diagnostics.Debug.WriteLine($"Service: {service.Uuid}");
                        var characteristicsResult = await service.GetCharacteristicsAsync();
                        foreach (var characteristic in characteristicsResult.Characteristics)
                        {
                            if (characteristic.Uuid.ToString() == "0000abf1-0000-1000-8000-00805f9b34fb")
                            {
                                byte[] dataToSend = new byte[] { 0x24, 0x4D, 0x3C, 0x08, 0xD6, 0xE8, 0x03, 0x24, 0x04, 0xE8, 0x03, 0xE8, 0x03, 0x15 };
                                var writeResult = await characteristic.WriteValueAsync(dataToSend.AsBuffer(), GattWriteOption.WriteWithResponse) ;

                                if (writeResult == GattCommunicationStatus.Success)
                                {
                                    System.Threading.Thread.Sleep(5000);
                                    dataToSend = new byte[] { 0x24, 0x4D, 0x3C, 0x08, 0xD6, 0xE8, 0x03, 0xE8, 0x03, 0xE8, 0x03, 0xE8, 0x03, 0xDE };
                                    writeResult = await characteristic.WriteValueAsync(dataToSend.AsBuffer(), GattWriteOption.WriteWithResponse);
                                }
                                else
                                {
                                    MessageBox.Show("Failed to write data to the device.");
                                }
                            }
                            System.Diagnostics.Debug.WriteLine($"characteristic: {characteristic.Uuid}");

                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to get GATT services.");
                }
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

		private void textBox2_TextChanged(object sender, EventArgs e)
		{

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

		private void button2_Click(object sender, EventArgs e)
		{
			client.SendCommand(MSPClient.MSPCommand.MSP_SET_MOTOR, new MotorSpeedRequest() { motor1Speed = 1000, motor2Speed = 1000, motor3Speed = 1000, motor4Speed = 1000 });
		}

		private void button1_Click_1(object sender, EventArgs e)
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

		private void button3_Click(object sender, EventArgs e)
		{
			
        }

        public static float HexToFloatLittleEndian(string hex)
		{
			if (hex.Length != 8)
				throw new ArgumentException("Hex string must be exactly 8 characters long.");

			// Конвертація hex-рядка в масив байт
			byte[] bytes = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}

			// Переконуємось, що байти вже в little-endian форматі
			if (BitConverter.IsLittleEndian == false)
				Array.Reverse(bytes); // Перевертаємо байти, якщо система не little-endian

			// Конвертуємо байти в float згідно IEEE 754
			return BitConverter.ToSingle(bytes, 0);
		}

        private void checkOn_CheckedChanged(object sender, EventArgs e)
        {
            ushort data =  checkOn.Checked ? (ushort)1 : (ushort)0;
          
            client.SendCommand(MSPClient.MSPCommand.MSP_SET_TEST, new CustomRequest() { turnOn = data, printDebug = checkPrint.Checked ? (ushort)1 : (ushort)0, delay = Convert.ToUInt16(txtDelay.Text) });
        }

        private void txtDelay_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
			

        }

		private void ProcessSafeMode()
		{
			while (true)
			{
				client.SendCommand(MSPClient.MSPCommand.MSP_ALTITUDE, new byte[] { });
			}
		}
    }
}
