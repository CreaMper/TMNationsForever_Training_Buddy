using LogicStorage.Dtos;
using Newtonsoft.Json;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Executor
{
    class Program
    {
        static void Main(string[] args)
        {
            var devices = CaptureDeviceList.Instance;
            var device = devices.First(x => x.Name.Equals(args[1]));

            var process = Process.GetProcessById(Int32.Parse(args[0]));

            if (device == null || process == null)
                throw new Exception("Device or process not found");

            device.Open(DeviceModes.Promiscuous, 1000);

            while (true)
            {
                var status = device.GetNextPacket(out PacketCapture e);
                if (status != GetPacketStatus.PacketRead)
                    continue;

                var rawCapture = e.GetPacket();

                var p = Packet.ParsePacket(rawCapture.GetLinkLayers(), rawCapture.Data);

                var packetString = p.ToString();

                if (packetString.Contains($" SourceAddress={GetLocalIPAddress()}"))
                    continue;

                var dataString = BytesToStringConverted(rawCapture.Data);
                if (!dataString.Contains("<header"))
                    continue;

                var trackInfo = DataStringDeserialise(dataString);

                if (trackInfo.Equals(""))
                {
                    Console.WriteLine("ERROR WITH DATA FETCH!");
                    continue;
                }

                var trackId = XasecoCheck(trackInfo);

                if (trackId.Equals(""))
                {
                    Console.WriteLine("ERROR WITH DATA FETCH!");
                    continue;
                }

                var packageUrl = XmaniaDownload(trackId);

                if (packageUrl.Equals(""))
                {
                    Console.WriteLine("ERROR WITH DATA FETCH!");
                    continue;
                }

                LoadPackage(packageUrl);

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

        private static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static TrackDataDto DataStringDeserialise(string dataString)
        {
            var headerString = string.Empty;
            var pFrom = 0;
            var pTo = 0;

            try
            {
                pFrom = dataString.IndexOf("<header ");
                pTo = dataString.LastIndexOf("</header>") + "</header>".Length;

                headerString = dataString.Substring(pFrom, pTo - pFrom);
            }
            catch
            {
                Console.WriteLine("Header packet is corrupted, checking an ident...");
                headerString = dataString;
            }


            pFrom = headerString.IndexOf("<ident ") + "<ident ".Length;
            pTo = headerString.LastIndexOf("/><desc");

            var indentString = headerString.Substring(pFrom, pTo - pFrom);

            var escapedUid = indentString.Replace("uid=\"", "");
            var escapedName = escapedUid.Replace("\" name=\"", "%^%^");
            var escapedAuthor = escapedName.Replace("\" author=\"", "%^%^");
            var removeLastChar = escapedAuthor.Remove(escapedAuthor.Length-1);

            var splited = removeLastChar.Split("%^%^");

            var track = new TrackDataDto()
            {
                UID = splited[0],
                TrackName = splited[1],
                AuthorName = splited[2]
            };
            Console.WriteLine($"Found a new track packet!");
            Console.WriteLine($"UID = {track.UID} TRACKNAME={track.TrackName} AUTHOR={track.AuthorName}");


            return track;
        }

        private static string XasecoCheck(TrackDataDto track)
        {
            Console.WriteLine();
            Console.WriteLine("Checking Xaseco for data...");

            var client = new HttpClient();

            var request = new XasecoRequestDto()
            {
                Uid = track.UID,
                Get = "Find UId!"
            };

            var response = client.GetAsync($"https://www.xaseco.org/uidfinder.php?uid={track.UID}").Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                string responseString = responseContent.ReadAsStringAsync().Result;

                Console.WriteLine($"Status: {response.StatusCode}");

                var splitedContent = responseString.Split("&id=");
                var split = splitedContent[1].Split("\">TMX");
                var onlyId = split[0];

                Console.WriteLine(onlyId);
                Console.WriteLine($"https://tmnf.exchange/trackshow/{onlyId}");

                return onlyId;

            } else
            {
                return "";
            }
        }

        private static string XmaniaDownload(string trackId)
        {
            var client = new HttpClient();

            var response = client.GetAsync($"https://tmnf.exchange/api/replays?trackId={trackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt").Result;

            Console.WriteLine($"Reading xmania....");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                string responseString = responseContent.ReadAsStringAsync().Result;

                Console.WriteLine($"Status: {response.StatusCode}");

                var root = JsonConvert.DeserializeObject<TrackStatsDto>(responseString);

                Console.WriteLine(root.Results.First().ReplayId);

                Console.WriteLine($"https://tmnf.exchange/recordgbx/{root.Results.First().ReplayId}");

                return $"https://tmnf.exchange/recordgbx/{root.Results.First().ReplayId}";
            }

            return "";
        }

        private static void LoadPackage(string packageUrl)
        {
            if (File.Exists("replay.gbx"))
                File.Delete("replay.gbx");

            var client = new HttpClient();

            var response = client.GetAsync(packageUrl).Result;
            Console.WriteLine($"Downloading package");

            if (response.IsSuccessStatusCode)
            {
                var resultStream = response.Content.ReadAsStreamAsync().Result;
                var fileStream = File.Create($"replay.gbx");
                resultStream.CopyTo(fileStream);

                resultStream.Close();
                fileStream.Close();

                var p = new Process();
                p.StartInfo = new ProcessStartInfo($"replay.gbx")
                {
                    UseShellExecute = true
                };
                p.Start();

                Console.WriteLine($"Download finished, running replay...");
            }
        }
    }
}
