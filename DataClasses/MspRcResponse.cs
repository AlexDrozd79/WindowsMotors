using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspRcResponse : MSPResponse
	{
		public ushort Channel1 { get; set; }
		public ushort Channel2 { get; set; }
		public ushort Channel3 { get; set; }
		public ushort Channel4 { get; set; }
		public ushort Channel5 { get; set; }
		public ushort Channel6 { get; set; }
		public ushort Channel7 { get; set; }
		public ushort Channel8 { get; set; }

		public static MspRcResponse FromByteArray(byte[] payload)
		{
			byte[] data = payload.Skip(5).ToArray();
			data = data.Take(data.Length - 1).ToArray();
			if (data.Length < 16)
			{
				throw new ArgumentException("Invalid payload length. Expected at least 16 bytes for 8 channels.");
			}

			MspRcResponse response = new MspRcResponse();

			// Parse each channel's value (2 bytes per channel, little-endian)
			response.Channel1 = BitConverter.ToUInt16(data, 0);
			response.Channel2 = BitConverter.ToUInt16(data, 2);
			response.Channel3 = BitConverter.ToUInt16(data, 4);
			response.Channel4 = BitConverter.ToUInt16(data, 6);
			response.Channel5 = BitConverter.ToUInt16(data, 8);
			response.Channel6 = BitConverter.ToUInt16(data, 10);
			response.Channel7 = BitConverter.ToUInt16(data, 12);
			response.Channel8 = BitConverter.ToUInt16(data, 14);

			return response;
		}

		public override string ToString()
		{
			return $"Channel 1: {Channel1}, Channel 2: {Channel2}, Channel 3: {Channel3}, Channel 4: {Channel4}, " +
				   $"Channel 5: {Channel5}, Channel 6: {Channel6}, Channel 7: {Channel7}, Channel 8: {Channel8}";
		}
	}
}
