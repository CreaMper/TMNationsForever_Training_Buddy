using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

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

            if (packetRecieved == 0)
                return false;
            else
                return true;
        }

        public List<string> GetDeviceList(bool showAll)
        {
            var deviceNamesList = new List<string>();

            foreach (var device in DeviceList)
            {
                var deviceString = device.ToString();

                if (showAll)
                {
                    deviceNamesList.Add(device.Name);
                }
                if (deviceString.Contains("Friendly") && deviceString.Contains("1) "))
                {
                    deviceNamesList.Add(device.Name);
                }
            }

            return deviceNamesList;
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
