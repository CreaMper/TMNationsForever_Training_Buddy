using LogicStorage;
using LogicStorage.Utils;
using SharpPcap;
using System;
using System.Threading;

namespace Executor
{
    class Program
    {
        private static Factory _factory;

        static void Main(string[] args)
        {
            _factory = new Factory();

            if (!RuntimeCheck.Check(_factory))
            {
                Logger.Log("Initialisation failed! Exiting in 5 seconds...", LogTypeEnum.CRITICAL);
                Thread.Sleep(5000);
                Environment.Exit(69);
            }

            Console.Title = "TMNationsForever Training Buddy Executor | Created by CreaMper";

            _factory.Network.Device.Open(DeviceModes.Promiscuous, _factory.ExecutorConfig.ListeningIntensivityMiliseconds);

            while (true)
            {
                if (_factory.Client.Buddy.HasExited)
                {
                    Logger.Log("Client buddy has been closed! Exiting executor in 5 seconds...", LogTypeEnum.CRITICAL);
                    Thread.Sleep(5000);
                    System.Environment.Exit(1);
                }

                var packetData = PacketSniffer.Sniff(_factory.Network);
                if (packetData == null)
                    continue;

                var trackInfo = Converters.PacketStringToTrackDataConverter(packetData);
                if (trackInfo == null)
                    continue;

                Logger.LogSeparator();
                Logger.Log("Found a new map Packet!", LogTypeEnum.Success);
                Logger.Log($"Author: {trackInfo.AuthorName} | Trackname: {Converters.TrackNameConverter(trackInfo.TrackName)} | UID: {trackInfo.UID}", LogTypeEnum.Info);

                var trackIdAndSource = _factory.Request.GetTrackIdAndSource(trackInfo);
                if (trackIdAndSource == null) 
                {
                    Logger.Log("Cannot find a Track ID! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                Logger.Log("Track ID found!", LogTypeEnum.Success);
                Logger.Log($"Track ID: {trackIdAndSource.TrackId} | Source: {trackIdAndSource.Source}", LogTypeEnum.Info);

                var replayDataAndSource = _factory.Request.GetReplayId(trackIdAndSource);
                if (replayDataAndSource == null)
                {
                    Logger.Log("Cannot find a replay that would be updated to TMX! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                Logger.Log("Replay ID found!", LogTypeEnum.Success);
                Logger.Log($"Replay by: {replayDataAndSource.Author} | Time: {replayDataAndSource.Time} | ReplayId: {replayDataAndSource.ReplayId} | Source: {replayDataAndSource.Source}", LogTypeEnum.Info);

                var downloadSuccessful = _factory.Request.DownloadReplay(replayDataAndSource);
                if (!downloadSuccessful)
                {
                    Logger.Log("Error occured when downloading a replay file! Perhaps buddy doesn't have access to file?", LogTypeEnum.CRITICAL);
                    continue;
                }
                else
                {
                    Logger.Log("Replay downloaded successful! Injecting file to buddy client...", LogTypeEnum.Success);
                    _factory.Client.InjectReplay(_factory.Client.Buddy, replayDataAndSource);
                    continue;
                }
            }
        }
    }
}
