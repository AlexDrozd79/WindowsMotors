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
		// Properties representing the fields in the MSP_STATUS payload
		public ushort CycleTime { get; set; }
		public ushort I2cErrorCount { get; set; }
		public uint SensorPresent { get; set; }
		public uint FlightModeFlags { get; set; }
		public byte Profile { get; set; }
		public byte CpuLoad { get; set; }
		public ushort BatteryVoltage { get; set; } // in 0.1V increments
		public ushort BatteryMahDrawn { get; set; } // in milliampere-hours (mAh)
		public byte Rssi { get; set; }

		// Method to parse byte array to MSP_STATUS payload
		public static MSPStatusResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();

			if (data.Length != 22) // MSP_STATUS payload is 22 bytes long
			{
				throw new ArgumentException("Invalid MSP_STATUS payload length.");
			}

			MSPStatusResponse status = new MSPStatusResponse
			{
				CycleTime = BitConverter.ToUInt16(data, 0),          // 2 bytes
				I2cErrorCount = BitConverter.ToUInt16(data, 2),      // 2 bytes
				SensorPresent = BitConverter.ToUInt32(data, 4),      // 4 bytes
				FlightModeFlags = BitConverter.ToUInt32(data, 8),    // 4 bytes
				Profile = data[12],                                  // 1 byte
				CpuLoad = data[13],                                  // 1 byte
				BatteryVoltage = BitConverter.ToUInt16(data, 14),    // 2 bytes
				BatteryMahDrawn = BitConverter.ToUInt16(data, 16),   // 2 bytes
				Rssi = data[18]                                      // 1 byte
			};

			return status;
		}

		// Override ToString() to display the payload in a readable format
		public override string ToString()
		{
			return $"Cycle Time: {CycleTime} µs" + Environment.NewLine +
				   $"I2C Errors: {I2cErrorCount}" + Environment.NewLine +
				   $"Sensors Present: 0x{SensorPresent:X8}" + Environment.NewLine +
				   $"Flight Modes: 0x{FlightModeFlags:X8}" + Environment.NewLine +
				   $"Profile: {Profile}" + Environment.NewLine +
				   $"CPU Load: {CpuLoad}%" + Environment.NewLine +
				   $"Battery Voltage: {BatteryVoltage * 0.1}V" + Environment.NewLine +
				   $"Battery mAh Drawn: {BatteryMahDrawn} mAh" + Environment.NewLine +
				   $"RSSI: {Rssi}";
		}
	}

}
