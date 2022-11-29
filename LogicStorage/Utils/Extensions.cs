using PacketDotNet;
using SharpPcap;
using System.IO;
using System.Reflection;

namespace LogicStorage.Utils
{
    public static class Extensions
    {
        private static readonly MethodInfo GetLinkLayerType;

        static Extensions()
        {
            var propertyInfo = typeof(RawCapture).GetProperty("LinkLayerType", BindingFlags.Public | BindingFlags.Instance);
            GetLinkLayerType = propertyInfo?.GetMethod;
        }

        public static LinkLayers GetLinkLayers(this RawCapture rawCapture)
        {
            // Allows using PacketDotNet versions other than the one used by SharpPcap.
            return (LinkLayers)(GetLinkLayerType?.Invoke(rawCapture, null) ?? 0);
        }

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}
