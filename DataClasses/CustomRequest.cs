using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class CustomRequest : MSPRequest
	{
		public UInt16 turnOn { get; set; } = 0;
        public UInt16 printDebug { get; set; } = 0;
        public UInt16 delay { get; set; } = 3;

        public override byte[] ToByteArray()
		{
			byte[] byteArray = new byte[6];

            // Store the least significant byte first (little-endian)
            byteArray[0] = (byte)(turnOn & 0xFF);       // LSB
                                                             // Store the most significant byte
            byteArray[1] = (byte)((turnOn >> 8) & 0xFF); // MSB

            byteArray[2] = (byte)(printDebug & 0xFF);       // LSB
                                                             // Store the most significant byte
            byteArray[3] = (byte)((printDebug >> 8) & 0xFF); // MSB

            byteArray[4] = (byte)(delay & 0xFF);       // LSB
                                                             // Store the most significant byte
            byteArray[5] = (byte)((delay >> 8) & 0xFF); // MSB

            return byteArray;


		}
	}
}
