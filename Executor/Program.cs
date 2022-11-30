using LogicStorage.Utils;
using PacketDotNet;
using SharpPcap;
using System;
using System.Threading;

namespace Executor
{
    class Program : Helper
    {
        static void Main(string[] args)
        {
            if (!ProgramInitialize())
            {
                Log(_initFailMsg, LogTypeEnum.CRITICAL);
                Log("Please, run the configuration tool... Exiting in 5 seconds...", LogTypeEnum.Error);
                Thread.Sleep(5000);

                System.Environment.Exit(1);
            }

            _device.Open(DeviceModes.Promiscuous, (100 * _config.ListeningIntensivityLevel) - 1100);

            while (true)
            {
                if (_clientProcess.HasExited)
                {
                    Log("Client buddy has been closed! Exiting executor in 5 seconds...", LogTypeEnum.CRITICAL);
                    Thread.Sleep(5000);
                    System.Environment.Exit(1);
                }

                var packetStatus = _device.GetNextPacket(out PacketCapture pc);
                if (packetStatus != GetPacketStatus.PacketRead)
                    continue;

                var rawCapture = pc.GetPacket();
                var parsedPacket = Packet.ParsePacket(rawCapture.GetLinkLayers(), rawCapture.Data).ToString();

                if (!PacketCheck(parsedPacket))
                    continue;

                var dataString = BytesToStringConverted(rawCapture.Data);
                if (!PacketDataCheck(dataString))
                    continue;

                LogSeparator();

                var trackInfo = PacketDataToTrackDto(dataString);
                if (trackInfo == null)
                {
                    Log("Unfortunately data packet seems to be wrong! Please re-enter race! :(", LogTypeEnum.CRITICAL);
                    continue;
                }

                var xasecoApproach = DownloadReplayUsingXasecoApproach(trackInfo);
                if (xasecoApproach)
                {
                    InjectReplay();
                    continue;
                }

                var tmxApproach = DownloadReplayUsingTMXApproach(trackInfo);
                if (tmxApproach)
                {
                    InjectReplay();
                    continue;
                }
            }
        }

    }
}
