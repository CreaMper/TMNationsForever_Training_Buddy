using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMFN_Training_Buddy.Handlers;

namespace TMFN_Training_Buddy.Utils
{
    public class MonitorHandler
    {
        private NetworkHandler _network;
        private LogHandler _log;

        public bool start = false;

        public MonitorHandler(LogHandler log, NetworkHandler network)
        {
            _network = network;
            _log = log;
        }

        public void ListenerStart(ILiveDevice device)
        {
            while (true)
            {
                PacketCapture e;
                var status = device.GetNextPacket(out e);
                if (status != GetPacketStatus.PacketRead)
                    continue;

                var rawCapture = e.GetPacket();

                var p = Packet.ParsePacket(rawCapture.GetLinkLayers(), rawCapture.Data);

                var packetString = p.ToString();

                if (packetString.Contains($" SourceAddress={GetLocalIPAddress()}"))
                    continue;

                var dataString = BytesToStringConverted(rawCapture.Data);
                if (!dataString.Contains("<header") && !dataString.Contains("</header>"))
                    continue;

                DataStringDeserialise(dataString);

            }
        }

        public static string GetLocalIPAddress()
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

        private string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private void DataStringDeserialise(string dataString)
        {
            int pFrom = dataString.IndexOf("<header ");
            int pTo = dataString.LastIndexOf("</header>") + "</header>".Length;

            var headerData = dataString.Substring(pFrom, pTo - pFrom);

            _log.AddLog("Found a header!");
            _log.AddLog(headerData);
        }
    }
}
