using PacketDotNet;
using SharpPcap;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMFN_Training_Buddy.Utils;

namespace TMFN_Training_Buddy.Handlers
{
    public class NetworkHandler
    {
        public CaptureDeviceList DeviceList { get; set; }

        public NetworkHandler()
        {
            DeviceList = CaptureDeviceList.Instance;
        }


        public ILiveDevice SelectDevice()
        {
            return DeviceList.First();
        }

        public bool ChallangeInterface(ILiveDevice device)
        {
            device.Open(DeviceModes.Promiscuous, 100);

            var packetRecieved = 0;

            for (int i = 1; i < 11; i++)
            {
                PacketCapture e;
                var status = device.GetNextPacket(out e);

                if (status != GetPacketStatus.PacketRead)
                    continue;

                packetRecieved++;
            }

            if (packetRecieved == 0)
                return false;
            else
                return true;
        }
    }
}
