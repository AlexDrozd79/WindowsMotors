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
    internal class RcMonitor
    {
        private MSPClient client;
        private Thread thMonitor;
        public MspRcResponse RCData = null;

     

        public RcMonitor(MSPClient client)
        {
            this.client = client;
        }

        

        public void Monitor()
        {
            if (thMonitor == null)
            {
                thMonitor = new Thread(() =>
                {
                    while (true)
                    {
                        client.SendCommand(MSPClient.MSPCommand.MSP_RC, new byte[] { });
                        Thread.Sleep(100);
                    }
                });

                thMonitor.Start();
            }
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
            if (response is MspRcResponse)
            {
                RCData = (MspRcResponse)response;
            }
        }
    }
}
