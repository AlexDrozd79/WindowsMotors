using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMotors.DataClasses;

namespace WindowsFormsApp1.DataClasses
{
	public class MspArmingConfigResponse : MSPResponse
	{
		// Maximum arming angle in degrees; 0 disables the angle check
		public ushort MaxArmingAngle { get; set; }

		// Time in seconds after which the quadcopter disarms automatically if there's no throttle input
		public ushort DisarmDelay { get; set; }

		// Minimum throttle required for arming
		public ushort MinThrottle { get; set; }

		// Other fields can be added here depending on firmware version

		// Method to parse the arming config from a byte array (payload)
		public static MspArmingConfigResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();
			if (data.Length < 3) // Adjust length based on the number of fields
				throw new ArgumentException("Invalid payload length.");

			var config = new MspArmingConfigResponse
			{
				MaxArmingAngle = BitConverter.ToUInt16(payload, 0),
				DisarmDelay = BitConverter.ToUInt16(payload, 2),
				MinThrottle = BitConverter.ToUInt16(payload, 4)
			};

			return config;
		}

		// Convert back to byte array for sending configuration to the flight controller
		public byte[] ToByteArray()
		{
			var payload = new byte[6];

			Array.Copy(BitConverter.GetBytes(MaxArmingAngle), 0, payload, 0, 2);
			Array.Copy(BitConverter.GetBytes(DisarmDelay), 0, payload, 2, 2);
			Array.Copy(BitConverter.GetBytes(MinThrottle), 0, payload, 4, 2);

			return payload;
		}

		public override string ToString()
		{
			return $"Max Arming Angle: {MaxArmingAngle}, Disarm Delay: {DisarmDelay}, Min Throttle: {MinThrottle}";
		}
	}
}
