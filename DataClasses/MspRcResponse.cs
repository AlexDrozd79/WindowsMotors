using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
    public class MspRcResponse : MSPResponse
    {
        public ushort Roll { get; set; }
        public ushort Pitch { get; set; }
        public ushort Yaw { get; set; }
        public ushort Throttle { get; set; }
        public ushort Aux1 { get; set; }
        public ushort Aux2 { get; set; }
        public ushort Aux3 { get; set; }
        public ushort Aux4 { get; set; }

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
            response.Roll = BitConverter.ToUInt16(data, 0);
            response.Pitch = BitConverter.ToUInt16(data, 2);
            response.Yaw = BitConverter.ToUInt16(data, 4);
            response.Throttle = BitConverter.ToUInt16(data, 6);
            response.Aux1 = BitConverter.ToUInt16(data, 8);
            response.Aux2 = BitConverter.ToUInt16(data, 10);
            response.Aux3 = BitConverter.ToUInt16(data, 12);
            response.Aux4 = BitConverter.ToUInt16(data, 14);

            return response;
        }

        public override string ToString()
        {
            return $"Roll: {Roll}, Pitch: {Pitch}, Yaw: {Yaw}, Throttle: {Throttle}, " +
                   $"Channel 5: {Aux1}, Channel 6: {Aux2}, Channel 7: {Aux3}, Channel 8: {Aux4}";
        }
    }
}
