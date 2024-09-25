using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMotors.DataClasses;

namespace WindowsMotors.DataClasses
{

	public class MSPRawIMUResponse : MSPResponse
	{
		// Properties to hold the IMU values
		public int AccX { get; private set; }
		public int AccY { get; private set; }
		public int AccZ { get; private set; }
		public int GyroX { get; private set; }
		public int GyroY { get; private set; }
		public int GyroZ { get; private set; }
		public int MagX { get; private set; }
		public int MagY { get; private set; }
		public int MagZ { get; private set; }

		// Method to parse byte array to MSP_STATUS payload
		public static MSPRawIMUResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();

			if (data.Length != 18) // MSP_STATUS payload is 22 bytes long
			{
				throw new ArgumentException("Invalid MSP_STATUS payload length.");
			}

			MSPRawIMUResponse status = new MSPRawIMUResponse
			{
				AccX = BitConverter.ToInt16(data, 0),
				AccY = BitConverter.ToInt16(data, 2),
				AccZ = BitConverter.ToInt16(data, 4),

				// Parse gyroscope values (next 6 bytes)
				GyroX = BitConverter.ToInt16(data, 6),
				GyroY = BitConverter.ToInt16(data, 8),
				GyroZ = BitConverter.ToInt16(data, 10),

				// Parse magnetometer values (last 6 bytes)
				MagX = BitConverter.ToInt16(data, 12),
				MagY = BitConverter.ToInt16(data, 14),
				MagZ = BitConverter.ToInt16(data, 16),
			};

			return status;
		}

		// Override ToString() to display the payload in a readable format
		public override string ToString()
		{
			return $"Accelerometer: Acc X: {AccX}, Acc Y: {AccY}, Acc Z: {AccZ}" + Environment.NewLine +
				   $"Gyroscope: Gyro X: {GyroX}, Gyro Y: {GyroY}, Gyro Z: {GyroZ}" + Environment.NewLine +
				   $"Magnetometer: Mag X: {MagX}, Mag Y: {MagY}, Mag Z: {MagZ}";			
		}
	}

}
