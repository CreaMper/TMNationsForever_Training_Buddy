using LogicStorage.Dtos;
using LogicStorage.Dtos.TrackData;
using LogicStorage.Handlers;
using LogicStorage.Utils;
using PacketDotNet;
using SharpPcap;
using System;
using System.Diagnostics;
using System.Threading;

namespace Executor
{
    class Program
    {
        private static ExecutorConfigDto _config;
        private static Serializer _serializer;
        private static NetworkHandler _network;
        private static ClientHandler _client;
        private static RequestHandler _request;

        private static Process _clientProcess;
        private static ILiveDevice _device;

        private static string _initFailMsg = "";
        private static TrackStatsResultDto _trackRecord;

        static void Main(string[] args)
        {
            _network = new NetworkHandler();
            _serializer = new Serializer();
            _client = new ClientHandler();
            _request = new RequestHandler();

            Console.Title = "TMNationsForever Training Buddy Executor | Created by CreaMper";
            
            _config = _serializer.DeserializeExecutorConfig();
            if (!RuntimeCheck.Check(_config))
            {
                Logger.Log(_initFailMsg, LogTypeEnum.CRITICAL);
                Logger.Log("Please, run the configuration tool... Exiting in 5 seconds...", LogTypeEnum.Error);
                Thread.Sleep(5000);

                System.Environment.Exit(1);
            }

            _device.Open(DeviceModes.Promiscuous, (100 * _config.ListeningIntensivityLevel) - 1100);

            while (true)
            {
                if (_clientProcess.HasExited)
                {
                    Logger.Log("Client buddy has been closed! Exiting executor in 5 seconds...", LogTypeEnum.CRITICAL);
                    Thread.Sleep(5000);
                    System.Environment.Exit(1);
                }

                var packetStatus = _device.GetNextPacket(out PacketCapture pc);
                if (packetStatus != GetPacketStatus.PacketRead)
                    continue;

                var rawCapture = pc.GetPacket();
                var parsedPacket = Packet.ParsePacket(rawCapture.GetLinkLayers(), rawCapture.Data).ToString();

                if (_network.IsPacketFromCorrectSource(parsedPacket))
                    continue;

                var dataString = Converters.BytesToStringConverter(rawCapture.Data);
                if (!_network.IsPacketDataCorrect(dataString))
                    continue;

                Logger.LogSeparator();

                var trackInfo = _request.ExtractTrackDataFromRequest(dataString);
                if (trackInfo == null)
                {
                    Logger.Log("Unfortunately data packet seems to be wrong! Please re-enter race! :(", LogTypeEnum.CRITICAL);
                    continue;
                }

                var xasecoApproach = _request.DownloadReplayUsingXasecoApproach(trackInfo);
                if (xasecoApproach)
                {
                    _client.InjectReplay(_clientProcess, _trackRecord);
                    continue;
                }

                var tmxApproach = _request.DownloadReplayUsingTMXApproach(trackInfo);
                if (tmxApproach)
                {
                    _client.InjectReplay(_clientProcess, _trackRecord);
                    continue;
                }
            }
        }
    }
}
