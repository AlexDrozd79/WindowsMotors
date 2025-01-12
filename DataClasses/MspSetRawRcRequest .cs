using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspSetRawRcRequest : MSPRequest
	{
		public ushort Throttle { get; set; } = 1500; // Значення за замовчуванням
		public ushort Roll { get; set; } = 1500;     // Значення за замовчуванням
		public ushort Pitch { get; set; } = 1500;    // Значення за замовчуванням
		public ushort Yaw { get; set; } = 1500;      // Значення за замовчуванням
		public ushort Aux1 { get; set; } = 1400;
		public ushort Aux2 { get; set; } = 1400;
		public ushort Aux3 { get; set; } = 1400;
		public ushort Aux4 { get; set; } = 1400;




		public override byte[] ToByteArray()
		{
			byte[] byteArray = new byte[16];

			// Запаковуємо кожне значення каналу в масив корисного навантаження
			PackChannelValue(Roll, byteArray, 0);
			PackChannelValue(Pitch, byteArray, 2);
			PackChannelValue(Throttle, byteArray, 4);
			PackChannelValue(Yaw, byteArray, 6);
			PackChannelValue(Aux1, byteArray, 8);
			PackChannelValue(Aux2, byteArray, 10);
			PackChannelValue(Aux3, byteArray, 12);
			PackChannelValue(Aux4, byteArray, 14);





			return byteArray;
		}

		private void PackChannelValue(ushort value, byte[] payload, int index)
		{
			payload[index] = (byte)(value & 0xFF);        // Молодший байт
			payload[index + 1] = (byte)((value >> 8) & 0xFF); // Старший байт
		}
	}
}
