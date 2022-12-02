using LogicStorage;
using LogicStorage.Utils;
using PacketDotNet;
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
                throw new Exception("Executor init failed!");

            Console.Title = "TMNationsForever Training Buddy Executor | Created by CreaMper";

            _factory.Network.Device.Open(DeviceModes.Promiscuous, (100 * _factory.ExecutorConfig.ListeningIntensivityLevel) - 1100);

            while (true)
            {
                if (_factory.Client.BuddyClient.HasExited)
                {
                    Logger.Log("Client buddy has been closed! Exiting executor in 5 seconds...", LogTypeEnum.CRITICAL);
                    Thread.Sleep(5000);
                    System.Environment.Exit(1);
                }

                var packetStatus = _factory.Network.Device.GetNextPacket(out PacketCapture pc);
                if (packetStatus != GetPacketStatus.PacketRead)
                    continue;

                var rawCapture = pc.GetPacket();
                var parsedPacket = Packet.ParsePacket(rawCapture.GetLinkLayers(), rawCapture.Data).ToString();

                if (_factory.Network.IsPacketFromCorrectSource(parsedPacket))
                    continue;

                var dataString = Converters.BytesToStringConverter(rawCapture.Data);
                if (!_factory.Network.IsPacketDataCorrect(dataString))
                    continue;

                Logger.LogSeparator();

                var trackInfo = _factory.Request.ExtractTrackDataFromRequest(dataString);
                if (trackInfo == null)
                {
                    Logger.Log("Unfortunately data packet seems to be wrong! Please re-enter race! :(", LogTypeEnum.CRITICAL);
                    continue;
                }

                var xasecoApproach = _factory.Request.DownloadReplayUsingXasecoApproach(trackInfo);
                if (xasecoApproach)
                {
                    _factory.Client.InjectReplay(_factory.Client.BuddyClient);
                    continue;
                }

                var tmxApproach = _factory.Request.DownloadReplayUsingTMXApproach(trackInfo);
                if (tmxApproach)
                {
                    _factory.Client.InjectReplay(_factory.Client.BuddyClient);
                    continue;
                }
            }
        }
    }
}
