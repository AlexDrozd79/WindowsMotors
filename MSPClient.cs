using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using WindowsMotors.DataClasses;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Collections;
using WindowsFormsApp1.DataClasses;
using System.Diagnostics;

namespace WindowsMotors
{
    public class MSPClient
    {
        private SerialPort serialPort = new SerialPort();
        private bool disposed = false;
        Thread readThread;
        private bool continueThread = true;
        private bool useSerial = false;
        private GattCharacteristic characteristicWrite;
        private GattCharacteristic characteristicRead;

        public delegate void MSPMessageHandler(MSPClient sender, byte[] rawData);
        public event MSPMessageHandler onData;

        private bool cliMode = false;
        public bool isCLIMode
        {
            get
            {
                return cliMode;
            }
            set
            {
                if (useSerial && !serialPort.IsOpen)
                {
                    throw new Exception("Serial port is not opened");
                }

                SwitchToCLIMode(value);
                cliMode = value;
            }

        }

      
        public enum MSPCommand
        {
            MSP_ARMING_CONFIG = 61,
            MSP_STATUS = 101,
            MSP_RAW_IMU = 102,
            MSP_SERVO = 103,
            MSP_MOTOR = 104,
            MSP_RC = 105,
            MSP_ATTITUDE = 108,
            MSP_ALTITUDE = 109,
            MSP_DEBUG_DATA = 114,
            MSP_SET_AUTOPILOT_DATA = 115,
            MSP_SET_RAW_RC = 200,
            MSP_SET_MOTOR = 214,
            MSP_SET_TEST = 216
        }

        public MSPClient(bool useSerial)
        {
            this.useSerial = useSerial;
            if (useSerial)
            {
                InitSerialPort("COM3", 115200); // 150200);
            }
            else
            {
                InitBlueToothConnection();
            }

            readThread = new Thread(Read);
            readThread.Start();
        }

        public MSPClient(bool useSerial, string port, int baudrate)
        {
            this.useSerial = useSerial;
            if (useSerial)
            {
                InitSerialPort(port, baudrate);
            }

            readThread = new Thread(Read);
            readThread.Start();
        }

        public void SendCommand(string rowData)
        {
            byte[] arr = HexStringToByteArray(rowData);
            serialPort.Write(arr, 0, arr.Length);
        }

        public async void SendCommand(MSPCommand command, byte[] rowData)
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

            if (useSerial)
            {
                serialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.ToArray().Length);
            }
            else
            {
                var writeResult = await characteristicWrite.WriteValueAsync(bytesToSend.ToArray().AsBuffer(), GattWriteOption.WriteWithResponse);
                if (writeResult == GattCommunicationStatus.Success)
                {
                   // System.Diagnostics.Debug.Write("succesfully sent command " + bytesToSend.ToString());
                }
            }

        }

        public async void SendCommand(MSPCommand command, MSPRequest data)
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
            if (useSerial)
            {
                serialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.ToArray().Length);
            }
            else
            {
                var writeResult = await characteristicWrite.WriteValueAsync(bytesToSend.ToArray().AsBuffer(), GattWriteOption.WriteWithResponse);
                if (writeResult == GattCommunicationStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine("succesfully sent command to WIFI" + BitConverter.ToString(bytesToSend.ToArray()).Replace("-", " "));
                }
            }
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
            switch (command)
            {
                case MSPCommand.MSP_ARMING_CONFIG:
                    response = MspArmingConfigResponse.FromByteArray(rawData);
                    break;
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
                case MSPCommand.MSP_SET_RAW_RC:
                    response = MspSetRawRcResponse.FromByteArray(rawData);
                    break;
                case MSPCommand.MSP_DEBUG_DATA:
                    response = MspDebugDataResponse.FromByteArray(rawData);
                    break;
                
            }
            return response;
        }


        public void Close()
        {
            continueThread = false;
            if (useSerial)
            {
                serialPort.Close();
            }
            else
            {
                if (this.characteristicWrite != null)
                {
                    this.characteristicWrite.Service.Session.Dispose();
                }
            }
        }

        private void InitSerialPort(string port, int baudrate)
        {
            serialPort.PortName = port;
            serialPort.BaudRate = baudrate;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;
            serialPort.Open();
            //serialPort.WriteLine("#");
        }


        private async void InitBlueToothConnection()
        {

            byte[] bytes = { 0x9A, 0xE2, 0x14, 0x18, 0x85, 0x34, 0x00, 0x00 }; //BetaFPV
            //byte[] bytes = { 0x42, 0x28, 0x30, 0x99, 0x65, 0x80, 0x00, 0x00 }; //SpeedyBee
            ulong ID = BitConverter.ToUInt64(bytes, 0);

            var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(ID);

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
                                this.characteristicWrite = characteristic;
                            }
                            if (characteristic.Uuid.ToString() == "0000abf2-0000-1000-8000-00805f9b34fb")
                            {
                                this.characteristicRead = characteristic;
                                this.characteristicRead.ValueChanged += CharacteristicRead_ValueChanged;
                                var status = await characteristicRead.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                                if (status == GattCommunicationStatus.Success)
                                {
                                    Debug.WriteLine("Успішно підписалися на Notifications");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CharacteristicRead_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] inputData = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(inputData);
            // тут — тільки справжні оновлення
            onData?.Invoke(this, inputData);
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
        public async void Read()
        {
            while (continueThread)
            {
                try
                {
                    if (useSerial)
                    {
                        if (serialPort.BytesToRead > 0)
                        {
                            // Allocate buffer to hold the incoming data
                            byte[] buffer = new byte[serialPort.BytesToRead];

                            // Read the data into the buffer
                            int bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                            
                            onData?.Invoke(this, buffer);
                        }
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

        private async void SwitchToCLIMode(bool isCLIMode)
        {
            if (useSerial)
            {
                if (isCLIMode)
                {
                    serialPort.WriteLine("#");
                }
                else
                {
                    serialPort.WriteLine("exit");
                    Thread.Sleep(3000);
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                }
            }
            else
            {
                if (isCLIMode)
                {

                    var writeResult = await characteristicWrite.WriteValueAsync(Encoding.ASCII.GetBytes("#").AsBuffer(), GattWriteOption.WriteWithResponse);
                    if (writeResult == GattCommunicationStatus.Success)
                    {
                        System.Diagnostics.Debug.Write("# succesfully sent ");
                    }
                }
                else
                {
                    var writeResult = await characteristicWrite.WriteValueAsync(Encoding.ASCII.GetBytes("exit").AsBuffer(), GattWriteOption.WriteWithResponse);
                    if (writeResult == GattCommunicationStatus.Success)
                    {
                        System.Diagnostics.Debug.Write("'exit' succesfully sent ");

                    }
                }
            }

        }

    }
}
