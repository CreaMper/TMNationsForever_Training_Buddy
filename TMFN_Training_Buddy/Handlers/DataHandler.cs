using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;

namespace TMFN_Training_Buddy.Handlers
{
    public class DataHandler
    {
        public List<string> GetDeviceList(CaptureDeviceList deviceList, bool showAll)
        {
            var deviceNamesList = new List<string>();

            foreach (var device in deviceList)
            {
                var deviceString = device.ToString();

                if (showAll)
                {
                    deviceNamesList.Add(device.Name);
                }
                if (deviceString.Contains("Friendly") && deviceString.Contains("1) "))
                {
                    deviceNamesList.Add(device.Name);
                }
            }

            return deviceNamesList;
        }
    }
}
