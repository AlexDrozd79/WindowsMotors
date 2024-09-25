using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsMotors.DataClasses;

namespace WindowsMotors.DataClasses
{

	public class MSPServoResponse : MSPResponse
	{
		public int[] Servos { get; set; } 

		// Constructor to parse the raw servo data
		public static MSPServoResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();
			if (data.Length != 16)
			{
				throw new ArgumentException("Invalid MSP_SERVO response data. Must be 16 bytes.");
			}

			MSPServoResponse response = new MSPServoResponse();
			response.Servos = new int[8];

			// Parse the servo PWM values (each 16-bit integer)
			for (int i = 0; i < 8; i++)
			{
				response.Servos[i] = BitConverter.ToInt16(data, i * 2);
			}

			return response;
		}

		// Method to display the servo values (for debugging or logging)
		public override string ToString()
		{
			string output = string.Empty;
			for (int i = 0; i < Servos.Length; i++)
			{
				output += $"Servo {i + 1} PWM: {Servos[i]}" + Environment.NewLine;
			}
			return output;
		}
	}

}
