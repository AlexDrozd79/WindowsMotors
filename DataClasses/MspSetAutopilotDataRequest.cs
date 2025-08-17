using System;

namespace WindowsMotors.DataClasses
{
    public class MspSetAutopilotDataRequest : MSPRequest
    {
        public short DeltaX { get; set; } = 0;
        public short DeltaY { get; set; } = 0;

        public override byte[] ToByteArray()
        {
            byte[] payload = new byte[4];

            PackShortValue(DeltaX, payload, 0);
            PackShortValue(DeltaY, payload, 2);

            return payload;
        }

        private void PackShortValue(short value, byte[] payload, int index)
        {
            // Кастимо в unchecked ushort, але кладемо як LE (little endian)
            ushort uval = unchecked((ushort)value);
            payload[index] = (byte)(uval & 0xFF);        // молодший байт
            payload[index + 1] = (byte)((uval >> 8) & 0xFF); // старший байт
        }
    }
}
