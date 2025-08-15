using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using WindowsMotors;
using WindowsMotors.DataClasses;
using System.IO;

namespace WindowsFormsApp1
{
    internal class PositionAligner
    {
        private MSPClient client;
        private Thread thAutoPilot;
        private List<string> log = new List<string>();


        private MspDebugDataResponse debugData;

        public ushort Trottle { get; set; } = 1000;

        public delegate void UIUpdateHandler(MspDebugDataResponse mspAdjustedData);

        public event UIUpdateHandler onUpdateUI;

 

        public PositionAligner(MSPClient client)
        {
            this.client = client;
        }

        

        public void Monitor()
        {
            thAutoPilot = new Thread(() =>
            {
                while (true)
                {
                    client.SendCommand(MSPClient.MSPCommand.MSP_DEBUG_DATA, new byte[] { });
                    Thread.Sleep(100);
                    onUpdateUI?.Invoke(debugData);
                }
            });
            
            thAutoPilot.Start();
        }

        public void Stop()
        {
            if (thAutoPilot != null && thAutoPilot.IsAlive)
            {
                thAutoPilot.Abort();
                thAutoPilot = null;
            }
        }


        public void ProcessResponse(MSPResponse response)
        {
            if (response is MspDebugDataResponse)
            {
                debugData = (MspDebugDataResponse)response;
            }
        }
 

    }
}
