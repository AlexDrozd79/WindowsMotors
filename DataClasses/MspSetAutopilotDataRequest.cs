using System;

namespace WindowsMotors.DataClasses
{
    public class MspSetAutopilotDataRequest : MSPRequest
    {
        public short DeltaYaw { get; set; } = 0;
        public short DeltaPitch { get; set; } = 0;
        public short DeltaRoll { get; set; } = 0;
        public short InitialYaw { get; set; } = 0;
        public short InitialPitch { get; set; } = 0;
        public short InitialRoll { get; set; } = 0;
        public short FrameID { get; set; } = 0;
        public short ModeID { get; set; } = 0;

        public override byte[] ToByteArray()
        {
            byte[] payload = new byte[16];

            PackShortValue(DeltaYaw, payload, 0);
            PackShortValue(DeltaPitch, payload, 2);
            PackShortValue(DeltaRoll, payload, 4);
            PackShortValue(InitialYaw, payload, 6);
            PackShortValue(InitialPitch, payload, 8);
            PackShortValue(InitialRoll, payload, 10);
            PackShortValue(FrameID, payload, 12);
            PackShortValue(ModeID, payload, 14);

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
