using LogicStorage;
using LogicStorage.Dtos;
using LogicStorage.Utils;
using System;
using System.Diagnostics;
using System.Linq;

namespace Executor
{
    public static class RuntimeCheck
    {
        public static bool Check(Factory factory)
        {
            var failMessage = string.Empty;

            if (factory.ExecutorConfig == null)
            {
                Logger.Log("Cannot find configuration file, trying to start AUTO setup...", LogTypeEnum.Info);

                var deviceList = factory.Network.GetDeviceList(false);
                var deviceName = deviceList.FirstOrDefault();
                if (deviceName == null)
                {
                    failMessage = "Cannot find any suitable internet interface!";
                    return false;
                }
                var device = factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(deviceName));

                if (!factory.Network.ChallangeInterface(device))
                {
                    failMessage = "Interface Challange failed!";
                    return false;
                }

                var process = factory.Client.GetGameClientProcess();
                if (process == null)
                {
                    failMessage = "Cannot find a running Buddy client!";
                    return false;
                }

                Logger.Log("Program initialized successfully from AUTO configuration!", LogTypeEnum.Success);

                factory.Network.Device = device;
                factory.Client.Buddy = process;

                factory.ExecutorConfig = new ExecutorConfigDto()
                {
                    ClientPID = process.Id,
                    NetworkInterfaceName = device.Name,
                    ListeningIntensivityLevel = 5,
                    MinimaliseExecutor = false
                };

                Logger.Log("Since configuration files was not found, executor will stay un-minimalised!", LogTypeEnum.Info);
            }
            else
            {
                Logger.Log("Found a configuration file!", LogTypeEnum.Info);

                try
                {
                    factory.Client.Buddy = Process.GetProcessById(factory.ExecutorConfig.ClientPID);
                    if (factory.Client.Buddy.HasExited)
                    {
                        failMessage = "It seems that you closed a Buddy Client!";
                        return false;
                    }

                    factory.Network.Device = factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(factory.ExecutorConfig.NetworkInterfaceName));
                }
                catch
                {
                    failMessage = "Configuration file data is obsolete/corrupted!";
                    return false;
                }

                Logger.Log("Program initialized successfully from a configuration file!", LogTypeEnum.Success);
            }

            Console.WriteLine();

            return true;
        }
    }
}
