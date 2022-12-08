using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace LogicStorage.Handlers
{
    public class NetworkHandler
    {
        public CaptureDeviceList DeviceList { get; set; }
        private Ping _ping;

        public NetworkHandler()
        {
            DeviceList = CaptureDeviceList.Instance;
            _device = null;
            _ping = new Ping();
        }

        private ILiveDevice _device;
        public ILiveDevice Device
        {
            get { return _device; }
            set { _device = value; }
        }

        public void AutoDeviceSelection()
        {
            foreach (var device in DeviceList)
                if (ChallangeInterface(device))
                    _device = device;
        }

        public bool ChallangeInterface(ILiveDevice device)
        {
            if (device == null)
                return false;

            device.Open(DeviceModes.Promiscuous, 100);
            PingDNS();

            var packetRecieved = 0;

            for (int i = 1; i < 11; i++)
            {
                PacketCapture e;
                var status = device.GetNextPacket(out e);

                if (status != GetPacketStatus.PacketRead)
                    continue;

                packetRecieved++;
                Thread.Sleep(100);
            }

            device.Close();
            if (packetRecieved == 0)
                return false;
            else
                return true;
        }

        private void PingDNS()
        {
            for (int i = 0; i < 11; i++)
                _ping.Send("8.8.8.8", 100);
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
