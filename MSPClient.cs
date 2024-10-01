using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WindowsMotors.DataClasses;

namespace WindowsMotors
{
	public class MSPClient
	{
		private SerialPort serialPort = new SerialPort();
		private bool disposed = false;
		Thread readThread;
		private bool continueThread = true;

		public delegate void MSPMessageHandler(MSPClient sender, byte[] rawData);

		public event MSPMessageHandler onData;

		public enum MSPCommand
		{
			MSP_STATUS = 101,
			MSP_RAW_IMU = 102,
			MSP_SERVO = 103,
			MSP_MOTOR = 104,
			MSP_RC = 105,
			MSP_ATTITUDE = 106,
			MSP_ALTITUDE  = 109,
			MSP_SET_MOTOR = 214
		}

		public MSPClient()
		{
			serialPort.PortName = "COM3";
			serialPort.BaudRate = 150200;
			serialPort.Parity = Parity.None;
			serialPort.StopBits = StopBits.One;
			serialPort.DataBits = 8;
			serialPort.ReadTimeout = 1000;
			//serialPort.WriteTimeout = 1000;
			serialPort.Open();

			readThread = new Thread(Read);
			readThread.Start();
		}

		public MSPClient(string port, int baudrate)
		{
			serialPort.PortName = port;
			serialPort.BaudRate = baudrate;
			serialPort.Parity = Parity.None;
			serialPort.StopBits = StopBits.One;
			serialPort.ReadTimeout = 1000;
			serialPort.WriteTimeout = 1000;
			serialPort.Open();

			readThread = new Thread(Read);
			readThread.Start();
		}

		public void SendCommand(string rowData)
		{
			byte[] arr = HexStringToByteArray(rowData);
			serialPort.Write(arr, 0, arr.Length);
		}

		public void SendCommand(MSPCommand command, byte[] rowData)
		{
			List<byte> bytesToSend = new List<byte>();
			bytesToSend.Add(Convert.ToByte('$'));
			bytesToSend.Add(Convert.ToByte('M'));
			bytesToSend.Add(Convert.ToByte('<'));
			bytesToSend.Add((byte)rowData.Length);
			bytesToSend.Add((byte)command);
			bytesToSend.AddRange(rowData);
			byte checkSum = CalculateXORChecksum(bytesToSend.GetRange(3, bytesToSend.Count - 3).ToArray());
			bytesToSend.Add(checkSum);
			serialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.ToArray().Length);
		}

		public void SendCommand(MSPCommand command, MSPRequest data)
		{
			byte[] rowData = data.ToByteArray();

			List<byte> bytesToSend = new List<byte>();
			bytesToSend.Add(Convert.ToByte('$'));
			bytesToSend.Add(Convert.ToByte('M'));
			bytesToSend.Add(Convert.ToByte('<'));
			bytesToSend.Add((byte)rowData.Length);
			bytesToSend.Add((byte)command);
			bytesToSend.AddRange(rowData);
			byte checkSum = CalculateXORChecksum(bytesToSend.GetRange(3, bytesToSend.Count - 3).ToArray());
			bytesToSend.Add(checkSum);
			serialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.ToArray().Length);
		}

		public static MSPCommand RetrieveCommand(byte[] rawData)
		{
			byte commandByte = rawData[4];
			return (MSPCommand)commandByte;
		}

		public static MSPResponse ParseResponse(byte[] rawData)
		{
			MSPResponse response = new MSPResponse();
			MSPCommand command = (MSPCommand)rawData[4];
			switch	(command) {
				case MSPCommand.MSP_STATUS:
					response = MSPStatusResponse.FromByteArray(rawData); 
					break;
				case MSPCommand.MSP_RAW_IMU:
					response = MSPRawIMUResponse.FromByteArray(rawData);
					break;
				case MSPCommand.MSP_SERVO:
					response = MSPServoResponse.FromByteArray(rawData);
					break;
				case MSPCommand.MSP_MOTOR:
					response = MspMotorResponse.FromByteArray(rawData);
					break;
				case MSPCommand.MSP_RC:
					response = MspRcResponse.FromByteArray(rawData);
					break;
				case MSPCommand.MSP_ATTITUDE:
					response = MspAttitudeResponse.FromByteArray(rawData);
					break;
				case MSPCommand.MSP_ALTITUDE:
					response = MspAltitudeResponse.FromByteArray(rawData);
					break;
			}
			return response;
		}


		public void Close()
		{
			continueThread = false;
			serialPort.Close();
		}

		private static byte[] HexStringToByteArray(string hex)
		{
			// Remove any spaces from the hex string
			hex = hex.Replace(" ", "");

			// Convert hex string to byte array
			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length; i += 2)
			{
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return bytes;
		}

		private static byte CalculateXORChecksum(byte[] data)
		{
			byte checksum = 0;
			foreach (byte b in data)
			{
				checksum ^= b; // XOR operation
			}
			return checksum;
		}


		public void Read()
		{
			while (continueThread)
			{
				try
				{
					if (serialPort.BytesToRead > 0)
					{
						// Allocate buffer to hold the incoming data
						byte[] buffer = new byte[serialPort.BytesToRead];

						// Read the data into the buffer
						int bytesRead = serialPort.Read(buffer, 0, buffer.Length);

						// Display the byte array in hexadecimal format
						System.Diagnostics.Debug.WriteLine("Received {0} bytes: {1}", bytesRead, BitConverter.ToString(buffer));

						onData?.Invoke(this, buffer);
					}
					 


				}
				catch (TimeoutException)
				{ 
					
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
		}


	}
}
