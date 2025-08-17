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
    internal class PositionMonitor
    {
        private MSPClient client;
        private Thread thMonitor;
        private List<string> log = new List<string>();


        private MspDebugDataResponse debugData;

        public ushort Trottle { get; set; } = 1000;

        public delegate void UIUpdateHandler(MspDebugDataResponse mspAdjustedData);

        public event UIUpdateHandler onUpdateUI;

        public bool isActive
        { 
            get { return thMonitor != null; }
        }

        public PositionMonitor(MSPClient client)
        {
            this.client = client;
        }

        

        public void Monitor()
        {
            thMonitor = new Thread(() =>
            {
                while (true)
                {
                    client.SendCommand(MSPClient.MSPCommand.MSP_DEBUG_DATA, new byte[] { });
                    Thread.Sleep(100);
                    onUpdateUI?.Invoke(debugData);
                }
            });
            
            thMonitor.Start();
        }

        public void Stop()
        {
            if (thMonitor != null && thMonitor.IsAlive)
            {
                thMonitor.Abort();
                thMonitor = null;
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
