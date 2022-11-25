using SharpPcap;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace LogicStorage.Handlers
{
    public class NetworkHandler
    {
        public CaptureDeviceList DeviceList { get; set; }
        private static HttpClient _httpClient { get; set; }

        public NetworkHandler()
        {
            DeviceList = CaptureDeviceList.Instance;
            _httpClient = new HttpClient();
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

        public string HttpRequestAsStringSync(string url)
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                return null;
        }

        public Stream HttpRequestAsStreamSync(string url)
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStreamAsync().Result;
            else
                return null;
        }
    }
}
