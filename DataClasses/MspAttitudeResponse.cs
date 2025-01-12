using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspAttitudeResponse : MSPResponse
	{
		public float Roll { get; set; }  // Roll in degrees
		public float Pitch { get; set; } // Pitch in degrees
		public float Yaw { get; set; }   // Yaw in degrees

		public static MspAttitudeResponse FromByteArray(byte[] payload)
		{
			// Skip the first 5 bytes (header, length, command) and remove the last byte (checksum)
			byte[] data = payload.Skip(5).Take(payload.Length - 6).ToArray();

			if (data.Length != 6)
			{
				throw new ArgumentException("Invalid payload length. Expected 6 bytes for attitude data.");
			}

			MspAttitudeResponse response = new MspAttitudeResponse();

			// Parse roll (signed 16-bit, tenths of degrees)
			short rollRaw = BitConverter.ToInt16(data, 0);
			response.Roll = rollRaw / 10.0f; // Convert to degrees

			// Parse pitch (signed 16-bit, tenths of degrees)
			short pitchRaw = BitConverter.ToInt16(data, 2);
			response.Pitch = pitchRaw / 10.0f; // Convert to degrees

			// Parse yaw (unsigned 16-bit, degrees)
			ushort yawRaw = BitConverter.ToUInt16(data, 4);
			response.Yaw = yawRaw; // Yaw is already in degrees

			return response;
		}

		public override string ToString()
		{
			return $"Roll: {Roll}°, Pitch: {Pitch}°, Yaw: {Yaw}°";
		}
	}
}
