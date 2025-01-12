using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspAltitudeResponse : MSPResponse
	{
		public float Altitude { get; set; }     // Altitude in meters
		public float Variometer { get; set; }   // Rate of climb/descent in cm/s

		public static MspAltitudeResponse FromByteArray(byte[] payload)
		{
			// Skip the first 5 bytes (header, length, command) and remove the last byte (checksum)
			byte[] data = payload.Skip(5).Take(payload.Length - 6).ToArray();

			if (data.Length != 6)
			{
				throw new ArgumentException("Invalid payload length. Expected 6 bytes for altitude data.");
			}

			MspAltitudeResponse response = new MspAltitudeResponse();

			// Parse altitude (signed 32-bit, in centimeters)
			int altitudeRaw = BitConverter.ToInt32(data, 0);
			response.Altitude = altitudeRaw / 100.0f; // Convert to meters

			// Parse variometer (signed 16-bit, in cm/s)
			short variometerRaw = BitConverter.ToInt16(data, 4);
			response.Variometer = variometerRaw; // Already in cm/s

			return response;
		}

		public override string ToString()
		{
			return $"Altitude: {Altitude} meters, Variometer: {Variometer} cm/s";
		}
	}
}
