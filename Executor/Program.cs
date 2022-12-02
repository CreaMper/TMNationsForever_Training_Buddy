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
                Logger.Log("Found a new map Packet!", LogTypeEnum.Info);
                Logger.Log($"Author: {trackInfo.AuthorName} | Trackname: {trackInfo.TrackName} | UID: {trackInfo.UID}", LogTypeEnum.Info);

                var trackIdAndSource = _factory.Request.GetTrackIdAndSource(trackInfo);
                if (trackIdAndSource == null)
                    continue;

                var replayDataAndSource = _factory.Request.GetReplayId(trackIdAndSource);
                if (replayDataAndSource == null)
                    continue;

                var downloadSuccessful = _factory.Request.DownloadReplay(replayDataAndSource);
                if (downloadSuccessful)
                {
                    _factory.Client.InjectReplay(_factory.Client.Buddy, replayDataAndSource);
                    continue;
                }
            }
        }
    }
}
