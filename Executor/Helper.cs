using LogicStorage.Dtos;
using LogicStorage.Dtos.SearchQuery;
using LogicStorage.Dtos.TrackData;
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
        public static ExecutorConfigDto _config;
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
                    NetworkInterfaceName = device.Name,
                    ListeningIntensivityLevel = 5,
                    MinimaliseExecutor = false
                };

                Console.WriteLine("Since configuration files was not found, executor will stay un-minimalised!");
            } 
            else
            {
                Console.WriteLine("Found a configuration file!");

                try
                {
                    _clientProcess = Process.GetProcessById(_config.ClientPID);
                    if (_clientProcess.HasExited)
                    {
                        _initFailMsg = "It seems that you closed a Buddy Client!";
                        return false;
                    }
                        
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

            if (response.Contains("This UId cannot be found on TMX"))
            {
                Console.WriteLine("Unfortunatelly, Xaseco does not have this map mapped to TMX data :(");
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

        public static bool DownloadReplayUsingXasecoApproach(TrackDataDto trackInfo)
        {
            var trackId = XasecoCheck(trackInfo);
            if (trackId == null)
            {
                Console.WriteLine("Failed to find an TMX ID data.");
                return false;
            }

            var download = DownloadReplayFromTMX(trackId);
            if (!download)
            {
                Console.WriteLine("Package download failed!");
                return false;
            }

            return true;
        }

        public static string ConvertTrackName(string name)
        {
            var specialCharToFilter = new List<string>()
            {
                "$w",
                "$n",
                "$o",
                "$i",
                "$t",
                "$s",
                "$g",
                "$z"
            };

            foreach (var item in specialCharToFilter)
                name = name.Replace(item, "");

            if (name.Contains("$$"))
                name = name.Replace("$$", "$");

            var splittedName = name.Split("$", StringSplitOptions.RemoveEmptyEntries);
            if (splittedName.Count() == 1)
                return name;

            var parsedName = "";
            foreach (var split in splittedName)
            {
                var parseColor = $"{split[0]}{split[1]}{split[2]}";
                if (IsHex(parseColor))
                    split.Remove(0, 2);

                parsedName += split;
            }

            for (int i = 0; i < splittedName.Count(); i++)
            {
                var parseColor = $"{splittedName[i][0]}{splittedName[i][1]}{splittedName[i][2]}";
                if (IsHex(parseColor))
                    splittedName[i] = splittedName[i].Remove(0, 3);
            }

            var gluedName = "";

            foreach (var item in splittedName)
                gluedName += item;

            return gluedName;
        }

        private static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

        public static bool DownloadReplayUsingTMXApproach(TrackDataDto trackInfo)
        {
            var convertedTrackName = ConvertTrackName(trackInfo.TrackName);
            Console.WriteLine($"Parsed Track name: {convertedTrackName}");

            var trackId = TMXCheck(convertedTrackName);
            if (trackId == null)
            {
                Console.WriteLine("Failed to find an TMX ID data.");
                return false;
            }

            var download = DownloadReplayFromTMX(trackId);
            if (!download)
            {
                Console.WriteLine("Package download failed!");
                return false;
            }

            return true;
        }

        private static string TMXCheck(string trackName)
        {
            Console.WriteLine("Fetching data from TMX...");

            var queryTrackName = ConvertTrackNameToQuery(trackName);

            var tmxSearchResult = $"https://tmnf.exchange/api/tracks?name={queryTrackName}&count=40&fields=TrackId%2CTrackName%2CAuthors%5B%5D%2CTags%5B%5D%2CAuthorTime%2CRoutes%2CDifficulty%2CEnvironment%2CCar%2CPrimaryType%2CMood%2CAwards%2CHasThumbnail%2CImages%5B%5D%2CIsPublic%2CWRReplay.User.UserId%2CWRReplay.User.Name%2CWRReplay.ReplayTime%2CWRReplay.ReplayScore%2CReplayType%2CUploader.UserId%2CUploader.Name";

            var response = _network.HttpRequestAsStringSync(tmxSearchResult);
            if (response == null)
                return null;

            var searchDto = JsonConvert.DeserializeObject<SearchDto>(response);

            return searchDto.Results.FirstOrDefault().TrackId.ToString();
        }

        private static string ConvertTrackNameToQuery(string trackName)
        {
            var convertRule = new List<List<string>>()
            {
                new List<string>(){ " ", "%20" },
                new List<string>(){ "!", "%21" },
                new List<string>(){ "#", "%23" },
                new List<string>(){ "$", "%24" },
                new List<string>(){ "%", "%25" },
                new List<string>(){ "&", "%26" },
                new List<string>(){ "'", "%27" },
                new List<string>(){ "(", "%28" },
                new List<string>(){ ")", "%29" },
                new List<string>(){ "*", "%2A" },
                new List<string>(){ "+", "%2B" },
                new List<string>(){ ",", "%2C" },
                new List<string>(){ "/", "%2F" },
                new List<string>(){ ":", "%3A" },
                new List<string>(){ ";", "%3B" },
                new List<string>(){ "=", "%3D" },
                new List<string>(){ "?", "%3F" },
                new List<string>(){ "@", "%40" },
                new List<string>(){ "[", "%5B" },
                new List<string>(){ "]", "%5D" },
            };

            var convertedTrackName = "";
            foreach (var character in trackName)
            {
                var characterToChange = convertRule.FirstOrDefault(x => x.Any(y => y[0].Equals(character)));
                if (characterToChange == null) 
                { 
                    convertedTrackName += character;
                    continue;
                }

                convertedTrackName += characterToChange[1];
            }

            return convertedTrackName;
        }
    }
}
