using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMotors.DataClasses;

namespace WindowsMotors.DataClasses
{

public class MSPStatusResponse : MSPResponse
{
		// Cycle time in microseconds
		public int CycleTime { get; set; }

		// I2C error count
		public int I2CErrorCount { get; set; }

		// Sensor status represented as a 32-bit integer
		public uint SensorStatus { get; set; }

		// Flags to represent various statuses (e.g., isArmed, etc.)
		public uint StatusFlags { get; set; }

		// Current configuration profile index
		public int CurrentConfigurationProfile { get; set; }

		// Box mode ID - could be an enumeration or specific mode flags
		public uint BoxModeId { get; set; }

		// RSSI value
		public int Rssi { get; set; }

		// Flight mode flags as a 16-bit integer
		public ushort FlightModeFlags { get; set; }

		// Method to parse byte array to MSP_STATUS payload
		public static MSPStatusResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();

			if (data.Length != 24) // MSP_STATUS payload is 22 bytes long
			{
				throw new ArgumentException("Invalid MSP_STATUS payload length.");
			}

			MSPStatusResponse status = new MSPStatusResponse
			{
				CycleTime = BitConverter.ToInt16(data, 0),
				I2CErrorCount = BitConverter.ToInt16(data, 2),
				SensorStatus = BitConverter.ToUInt32(data, 4),
				StatusFlags = BitConverter.ToUInt32(data, 8),
				CurrentConfigurationProfile = BitConverter.ToInt32(data, 12),
				BoxModeId = BitConverter.ToUInt32(data, 16),
				Rssi = BitConverter.ToInt16(data, 20),
				FlightModeFlags = BitConverter.ToUInt16(data, 22)
		};

			return status;
		}

		// Override ToString() to display the payload in a readable format
		public override string ToString()
		{
			return $"Cycle Time: {CycleTime}, I2C Error Count: {I2CErrorCount}, " +
			  $"Sensor Status: {SensorStatus}, Status Flags: {StatusFlags}, " +
			  $"Current Configuration Profile: {CurrentConfigurationProfile}, " +
			  $"Box Mode ID: {BoxModeId}, RSSI: {Rssi}, " +
			  $"Flight Mode Flags: {FlightModeFlags}";
		}
	}

}
