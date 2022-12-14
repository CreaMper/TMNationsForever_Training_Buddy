using PacketDotNet;
using SharpPcap;
using System.Collections.Generic;
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

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];

            list.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;

            list.Insert(newIndex, item);
        }
    }
}
