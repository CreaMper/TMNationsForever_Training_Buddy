using LogicStorage.Dtos;
using LogicStorage.Handlers;
using LogicStorage.Utils;
using Newtonsoft.Json;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Executor
{
    public class Helper
    {
        private static ExecutorConfigDto _config;
        private static Serializer _serializer;
        private static NetworkHandler _network;
        private static DLLImporter _importer;
        private static ClientHandler _client;
        private static string _packageUrl = "replay.gbx";

        public static Process _clientProcess;
        public static ILiveDevice _device;

        public static string _initFailMsg = "";

        public static bool ProgramInitialize()
        {
            _network = new NetworkHandler();
            _serializer = new Serializer();
            _client = new ClientHandler();
            _importer = new DLLImporter();

            Console.Title = "TMNationsForever Training Buddy Executor | Created by CreaMper";

            _config = _serializer.DeserializeExecutorConfig();
            if (_config == null)
            {
                Console.WriteLine("Cannot find configuration file, trying to start AUTO setup...");

                var deviceList = _network.GetDeviceList(false);
                var deviceName = deviceList.FirstOrDefault();
                if (deviceName == null)
                {
                    _initFailMsg = "Cannot find any suitable internet interface!";
                    return false;
                }
                var device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(deviceName));

                if (!_network.ChallangeInterface(device))
                {
                    _initFailMsg = "Interface Challange failed!";
                    return false;
                }

                var process = _client.GetGameClientProcess();
                if (process == null)
                {
                    _initFailMsg = "Cannot find a game client!";
                    return false;
                }

                Console.WriteLine("Program initialized successfully from AUTO configuration!");

                _device = device;
                _clientProcess = process;

                _config = new ExecutorConfigDto()
                {
                    ClientPID = process.Id,
                    NetworkInterfaceName = device.Name
                };
            } 
            else
            {
                Console.WriteLine("Found a configuration file!");

                try
                {
                    _clientProcess = Process.GetProcessById(_config.ClientPID);
                    if (_clientProcess.HasExited == true)
                        return false;
                    _device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(_config.NetworkInterfaceName));
                }
                catch
                {
                    _initFailMsg = "Configuration file data is obsolete/corrupted!";
                    return false;
                }

                Console.WriteLine("Program initialized successfully from a configuration file!");
            }

            Console.WriteLine($"Interface: {_device.Name}");
            Console.WriteLine($"Game Client PID: {_clientProcess.Id}");
            Console.WriteLine();

            return true;
        }

        public static bool PacketCheck(string parsedPacket)
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

        public static bool PacketDataCheck(string dataString)
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

        public static TrackDataDto PacketDataToTrackDto(string dataString)
        {
            var pFrom = dataString.IndexOf("<ident ") + "<ident ".Length;
            var pTo = dataString.LastIndexOf("/><desc");

            if (pTo - pFrom < 0)
                return null;

            var indentString = dataString.Substring(pFrom, pTo - pFrom);

            var escapedUid = indentString.Replace("uid=\"", "");
            var escapedName = escapedUid.Replace("\" name=\"", "%^%^");
            var escapedAuthor = escapedName.Replace("\" author=\"", "%^%^");
            var removeLastChar = escapedAuthor.Remove(escapedAuthor.Length - 1);

            var splited = removeLastChar.Split("%^%^");

            var track = new TrackDataDto()
            {
                UID = splited[0],
                TrackName = splited[1],
                AuthorName = splited[2]
            };

            Console.WriteLine();
            Console.WriteLine($"Found a new track packet!");
            Console.WriteLine($"UID = {track.UID} TRACKNAME={track.TrackName} AUTHOR={track.AuthorName}");

            return track;
        }

        public static string XasecoCheck(TrackDataDto track)
        {
            Console.WriteLine("Checking Xaseco for data...");

            var xasecoURL = $"https://www.xaseco.org/uidfinder.php?uid={track.UID}";

            var response = _network.HttpRequestAsStringSync(xasecoURL);
            if (response == null)
            {
                Console.WriteLine("Request failed! Loading aborted...");
                return null;
            }
            var splitedContent = response.Split("&id=");
            var split = splitedContent[1].Split("\">TMX");
            var onlyId = split[0];

            Console.WriteLine($"Found a track TXM ID: {onlyId}");
            return onlyId;
        }

        public static bool DownloadReplayFromTMX(string trackId)
        {
            Console.WriteLine("Fetching data form TMX...");

            var xasecoURL = $"https://tmnf.exchange/api/replays?trackId={trackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt";

            var response = _network.HttpRequestAsStringSync(xasecoURL);
            if (response == null)
                return false;

            var trackStats = JsonConvert.DeserializeObject<TrackStatsDto>(response);
            if (trackStats == null)
                return false;

            Console.WriteLine("Downloading package from TMX...");

            var topOneReplayId = trackStats.Results.FirstOrDefault().ReplayId;

            var apiDownloadLink = $"https://tmnf.exchange/recordgbx/{topOneReplayId}";

            var packageStream = _network.HttpRequestAsStreamSync(apiDownloadLink);
            if (packageStream == null)
                return false;

            if (File.Exists("replay.gbx"))
                File.Delete("replay.gbx");

            var fileStream = File.Create(_packageUrl);
            packageStream.CopyTo(fileStream);

            packageStream.Close();
            fileStream.Close();

            Console.WriteLine("Download complete! Injecting...");
            return true;
        }

        public static void InjectReplay()
        {
            _importer.UseSetForegroundWindow(_clientProcess.MainWindowHandle);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(_packageUrl)
            {
                UseShellExecute = true
            };
            p.Start();

            Console.WriteLine("Process Injected! Check a buddy client!");
        }

        public static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
