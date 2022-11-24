using SharpPcap;
using System.Linq;

namespace TMFN_Training_Buddy.Handlers
{
    public class NetworkHandler
    {
        public CaptureDeviceList DeviceList { get; set; }

        public NetworkHandler()
        {
            DeviceList = CaptureDeviceList.Instance;
        }


        public ILiveDevice SelectDevice()
        {
            return DeviceList.First();
        }
    }
}
