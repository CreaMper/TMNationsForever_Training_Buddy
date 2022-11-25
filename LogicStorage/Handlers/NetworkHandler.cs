using SharpPcap;
using System.Linq;

namespace LogicStorage.Handlers
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
