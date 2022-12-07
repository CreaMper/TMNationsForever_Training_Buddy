using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LogicStorage.Handlers
{
    public class NetworkHandler
    {
        public CaptureDeviceList _deviceList { get; set; }

        public NetworkHandler()
        {
            _deviceList = CaptureDeviceList.Instance;
            _device = null;
        }

        private ILiveDevice _device;
        public ILiveDevice Device
        {
            get { return _device; }
            set { _device = value; }
        }

        public void AutoDeviceSelection()
        {
            foreach (var device in _deviceList)
                if (ChallangeInterface(device))
                    _device = device;
        }

        public bool ChallangeInterface(ILiveDevice device)
        {
            if (device == null)
                return false;

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

            device.Close();
            if (packetRecieved == 0)
                return false;
            else
                return true;
        }

        public bool IsPacketFromCorrectSource(string parsedPacket)
        {
            var filteredContent = new List<string>()
            {
                $" SourceAddress={GetLocalIPAddress()}"
            };

            foreach (var item in filteredContent)
                if (parsedPacket.Contains(item))
                    return false;

            return true;
        }

        public bool IsPacketDataCorrect(string dataString)
        {
            var required = new List<string>()
            {
                "<header ",
                "</header>"
            };

            foreach (var item in required)
                if (!dataString.Contains(item))
                    return false;

            var notAllowed = new List<string>()
            {
                "<div/>"
            };

            foreach (var item in notAllowed)
                if (dataString.Contains(item))
                    return false;

            return true;
        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
