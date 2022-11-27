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
                Console.WriteLine("Program Initialize Failed!");
                Console.WriteLine(_initFailMsg);
                Console.WriteLine("Please, run the configuration tool... Exiting in 5 seconds...");
                Thread.Sleep(5000);

                System.Environment.Exit(1);
            }

            _device.Open(DeviceModes.Promiscuous, (100 * _config.ListeningIntensivityLevel) - 1100);

            while (true)
            {
                if (_clientProcess.HasExited)
                {
                    Console.WriteLine("Client buddy has been closed! Exiting executor in 5 seconds...");
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

                var trackInfo = PacketDataToTrackDto(dataString);
                if (trackInfo == null)
                {
                    Console.WriteLine("Track packet seems to be corrupted. Please, re-enter to race!");
                    continue;
                }

/*                var xasecoApproach = DownloadReplayUsingXasecoApproach(trackInfo);
                if (xasecoApproach)
                {
                    Console.WriteLine("Downloading replay usign Xaseco data fetch sucessful!");
                    InjectReplay();
                    continue;
                }*/

                var tmxApproach = DownloadReplayUsingTMXApproach(trackInfo);

            }
        }

    }
}
