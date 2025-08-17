using System;

namespace WindowsMotors.DataClasses
{
    public class MspSetAutopilotDataRequest : MSPRequest
    {
        public short DeltaYaw { get; set; } = 0;
        public short DeltaPitch { get; set; } = 0;
        public short InitialYaw { get; set; } = 0;
        public short InitialPitch { get; set; } = 0;
        public short FrameID { get; set; } = 0;

        public override byte[] ToByteArray()
        {
            byte[] payload = new byte[10];

            PackShortValue(DeltaYaw, payload, 0);
            PackShortValue(DeltaPitch, payload, 2);
            PackShortValue(InitialYaw, payload, 4);
            PackShortValue(InitialPitch, payload, 6);
            PackShortValue(FrameID, payload, 8);

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
