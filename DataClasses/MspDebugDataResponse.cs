using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
    public class MspDebugDataResponse : MSPResponse
    {
        public float Roll { get; set; }  // Roll in degrees
        public float Pitch { get; set; } // Pitch in degrees
        public float Yaw { get; set; }   // Yaw in degrees
        public float pidSetpointRoll { get; set; }  // Roll PID  
        public float pidSetpointPitch { get; set; } // Pitch PID  
        public float pidSetpointYaw { get; set; }   // Yaw PID  
        public float pidSumRoll { get; set; }  // Roll Sum  
        public float pidSumPitch { get; set; } // Pitch Sum
        public float pidSumYaw { get; set; }   // Yaw Sum  
        public float pidPRoll { get; set; }
        public float pidPPitch { get; set; }
        public float pidPYaw { get; set; }
        public float pidIRoll { get; set; }
        public float pidIPitch { get; set; }
        public float pidIYaw { get; set; }
        public float pidDRoll { get; set; }
        public float pidDPitch { get; set; }
        public float pidDYaw { get; set; }


        public static MspDebugDataResponse FromByteArray(byte[] payload)
        {
            // Skip the first 5 bytes (header, length, command) and remove the last byte (checksum)
            byte[] data = payload.Skip(5).Take(payload.Length - 6).ToArray();

            if (data.Length != 66)
            {
                throw new ArgumentException("Invalid payload length. Expected 6 bytes for attitude data.");
            }

            MspDebugDataResponse response = new MspDebugDataResponse();

            // Parse roll (signed 16-bit, tenths of degrees)
            short rollRaw = BitConverter.ToInt16(data, 0);
            response.Roll = rollRaw / 10.0f; // Convert to degrees

            // Parse pitch (signed 16-bit, tenths of degrees)
            short pitchRaw = BitConverter.ToInt16(data, 2);
            response.Pitch = pitchRaw / 10.0f; // Convert to degrees

            // Parse yaw (unsigned 16-bit, degrees)
            ushort yawRaw = BitConverter.ToUInt16(data, 4);
            response.Yaw = yawRaw; // Yaw is already in degrees

            float pidSetpointRoll = BitConverter.ToSingle(data, 6);
            response.pidSetpointRoll = pidSetpointRoll;

            // Parse pitch (signed 16-bit, tenths of degrees)
            float pidSetpointPitch = BitConverter.ToSingle(data, 10);
            response.pidSetpointPitch = pidSetpointPitch;

            // Parse yaw (unsigned 16-bit, degrees)
            float pidSetpointYaw = BitConverter.ToSingle(data, 14);
            response.pidSetpointYaw = pidSetpointYaw;

            float pidSumRoll = BitConverter.ToSingle(data, 18);
            response.pidSumRoll = pidSumRoll;

            // Parse pitch (signed 16-bit, tenths of degrees)
            float pidSumPitch = BitConverter.ToSingle(data, 22);
            response.pidSumPitch = pidSumPitch;

            // Parse yaw (unsigned 16-bit, degrees)
            float pidSumYaw = BitConverter.ToSingle(data, 26);
            response.pidSumYaw = pidSumYaw;

            float pidPRoll = BitConverter.ToSingle(data, 30);
            response.pidPRoll = pidPRoll;

            // Parse pitch (signed 16-bit, tenths of degrees)
            float pidPPitch = BitConverter.ToSingle(data, 34);
            response.pidPPitch = pidPPitch;

            // Parse yaw (unsigned 16-bit, degrees)
            float pidPYaw = BitConverter.ToSingle(data, 38);
            response.pidPYaw = pidPYaw;

            float pidIRoll = BitConverter.ToSingle(data, 42);
            response.pidIRoll = pidIRoll;

            // Parse pitch (signed 16-bit, tenths of degrees)
            float pidIPitch = BitConverter.ToSingle(data, 46);
            response.pidIPitch = pidIPitch;

            // Parse yaw (unsigned 16-bit, degrees)
            float pidIYaw = BitConverter.ToSingle(data, 50);
            response.pidIYaw = pidIYaw;

            float pidDRoll = BitConverter.ToSingle(data, 54);
            response.pidDRoll = pidDRoll;

            // Parse pitch (signed 16-bit, tenths of degrees)
            float pidDPitch = BitConverter.ToSingle(data, 58);
            response.pidDPitch = pidDPitch;

            // Parse yaw (unsigned 16-bit, degrees)
            float pidDYaw = BitConverter.ToSingle(data, 62);
            response.pidDYaw = pidDYaw;

            return response;
        }

        public override string ToString()
        {
            return $"Roll: {Roll}°, Pitch: {Pitch}°, Yaw: {Yaw}° pidSetpointRoll {pidSetpointRoll} pidSetpointPitch {pidSetpointPitch} pidSetpointYaw {pidSetpointYaw} " +
                $"pidSumRoll {pidSumRoll} pidSumPitch {pidSumPitch} pidSumYaw {pidSumYaw} pidPRoll {pidPRoll} pidPPitch {pidPPitch} pidPYaw {pidPYaw} " +
                $"pidIRoll {pidIRoll} pidIPitch {pidIPitch} pidIYaw {pidIYaw} pidDRoll {pidDRoll} pidDPitch {pidDPitch} pidDYaw {pidDYaw}" ;
        }
    }
}
