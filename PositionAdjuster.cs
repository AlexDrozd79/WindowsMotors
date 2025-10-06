using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class PositionAdjuster
    {

        private static float currentThrottle = 1000;
        public static float getSetpointStrength(short delta, short initialStrength)
        {
            float setPointValue = 0;
            short d = (short)(delta < 0 ? -delta : delta);
            if (d < 5 && d > -5)
            {
                setPointValue = 0;
            }
            else
            {
                setPointValue = d > 30 ? initialStrength : 60;
                setPointValue = delta > 0 ? setPointValue : -setPointValue;
            }
            return setPointValue;
        }

        public static float adjustYaw(short deltaYaw, short initialYaw)
        {
            return 1500 - getSetpointStrength(deltaYaw, initialYaw);
        }

        public static float adjustThrottle(short deltaPitch)
        {
            if (currentThrottle > 1299)
            {
                currentThrottle = 1300;
                return currentThrottle;
            }

            short d = (short)(deltaPitch < 0 ? -deltaPitch : deltaPitch);
            if (d > 5)
            {
                float addjustValue = 30f;
                currentThrottle = deltaPitch < 0 ? currentThrottle + addjustValue : currentThrottle - addjustValue;


            }
            return currentThrottle;
        }

    }
}
