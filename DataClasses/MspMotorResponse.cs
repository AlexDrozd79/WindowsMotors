using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspMotorResponse : MSPResponse
	{
		public ushort Motor1 { get; set; }
		public ushort Motor2 { get; set; }
		public ushort Motor3 { get; set; }
		public ushort Motor4 { get; set; }

		public static  MspMotorResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();

			if (data.Length < 8)
			{
				throw new ArgumentException("Invalid payload length. Expected at least 8 bytes for 4 motors.");
			}

			MspMotorResponse response = new MspMotorResponse();
			// Parse each motor's value (2 bytes per motor, little-endian)
			response.Motor1 = BitConverter.ToUInt16(data, 0); // Bytes 0 and 1
			response.Motor2 = BitConverter.ToUInt16(data, 2); // Bytes 2 and 3
			response.Motor3 = BitConverter.ToUInt16(data, 4); // Bytes 4 and 5
			response.Motor4 = BitConverter.ToUInt16(data, 6); // Bytes 6 and 7

			return response;
		}

		public override string ToString()
		{
			return $"Motor 1: {Motor1}, Motor 2: {Motor2}, Motor 3: {Motor3}, Motor 4: {Motor4}";
		}
	}
}
