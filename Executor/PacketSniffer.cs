using LogicStorage.Dtos.TrackData;
using LogicStorage.Handlers;
using LogicStorage.Utils;
using PacketDotNet;
using SharpPcap;

namespace Executor
{
    public static class PacketSniffer
    {
        public static string Sniff(NetworkHandler network)
        {
            var latestPacketStatus = network.Device.GetNextPacket(out PacketCapture pc);
            if (latestPacketStatus != GetPacketStatus.PacketRead)
                return null;

            var bytePacketData = pc.GetPacket();
            var stringPacketData = Packet.ParsePacket(bytePacketData.GetLinkLayers(), bytePacketData.Data).ToString();

            if (network.IsPacketFromCorrectSource(stringPacketData))
                return null;

            var packetString = Converters.BytesToStringConverter(bytePacketData.Data);
            if (network.IsPacketDataCorrect(packetString))
                return null;

            return packetString;
        }
    }
}
