using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MotorSpeedRequest: MSPRequest
	{
		public UInt16 motor1Speed { get; set; } = 1000;
		public UInt16 motor2Speed { get; set; } = 1000;
		public UInt16 motor3Speed { get; set; } = 1000;
		public UInt16 motor4Speed { get; set; } = 1000;
 

        public override byte[] ToByteArray()
		{
			byte[] byteArray = new byte[8];

			// Store the least significant byte first (little-endian)
			byteArray[0] = (byte)(motor1Speed & 0xFF);       // LSB
															  // Store the most significant byte
			byteArray[1] = (byte)((motor1Speed >> 8) & 0xFF); // MSB

			byteArray[2] = (byte)(motor2Speed & 0xFF);       // LSB
															 // Store the most significant byte
			byteArray[3] = (byte)((motor2Speed >> 8) & 0xFF); // MSB

			byteArray[4] = (byte)(motor3Speed & 0xFF);       // LSB
															 // Store the most significant byte
			byteArray[5] = (byte)((motor3Speed >> 8) & 0xFF); // MSB

			byteArray[6] = (byte)(motor4Speed & 0xFF);       // LSB
															 // Store the most significant byte
			byteArray[7] = (byte)((motor4Speed >> 8) & 0xFF); // MSB

			return byteArray;


		}
	}
}
