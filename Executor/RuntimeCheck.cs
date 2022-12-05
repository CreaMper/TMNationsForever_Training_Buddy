using LogicStorage;
using LogicStorage.Dtos;
using LogicStorage.Dtos.Config;
using LogicStorage.Utils;
using System.Diagnostics;
using System.Linq;

namespace Executor
{
    public static class RuntimeCheck
    {
        public static bool Check(Factory factory)
        {
            if (factory.ExecutorConfig == null)
            {
                Logger.Log("Cannot find configuration file, trying to start AUTO setup...", LogTypeEnum.Info);

                var availableDevicesList = factory.Network.GetDeviceList();
                var deviceName = availableDevicesList.FirstOrDefault();
                if (deviceName == null)
                {
                    Logger.Log("Cannot find any suitable internet interface!", LogTypeEnum.CRITICAL);
                    return false;
                }

                var device = factory.Network._deviceList.FirstOrDefault(x => x.Name.Equals(deviceName));
                if (!factory.Network.ChallangeInterface(device))
                {
                    Logger.Log("Interface Challange failed!", LogTypeEnum.CRITICAL);
                    return false;
                }

                var process = factory.Client.GetGameClientProcess();
                if (process == null)
                {
                    Logger.Log("Cannot find a running Buddy client!", LogTypeEnum.CRITICAL);
                    return false;
                }

                factory.Network.Device = device;
                factory.Client.Buddy = process;

                factory.ExecutorConfig = new ExecutorConfigDto()
                {
                    ClientPID = process.Id,
                    NetworkInterfaceName = device.Name,
                    ListeningIntensivityLevel = 5,
                    MinimaliseExecutor = false
                };

                Logger.Log("Program initialized successfully from AUTO configuration!", LogTypeEnum.Success);
                Logger.Log("Since configuration files was not found, executor will stay un-minimalised!", LogTypeEnum.Info);
            }
            else
            {
                Logger.Log("Found a configuration file!", LogTypeEnum.Info);

                try
                {
                    factory.Client.Buddy = Process.GetProcessById(factory.ExecutorConfig.ClientPID);
                }
                catch
                {
                    Logger.Log("Cannot find a previous Buddy client running!", LogTypeEnum.CRITICAL);
                    return false;
                }

                factory.Network.Device = factory.Network._deviceList.FirstOrDefault(x => x.Name.Equals(factory.ExecutorConfig?.NetworkInterfaceName));
                if (factory.Network.Device == null)
                {
                    Logger.Log("Network Interface name is corrupted!", LogTypeEnum.CRITICAL);
                    return false;
                }

                Logger.Log("Program initialized successfully from a configuration file!", LogTypeEnum.Success);
            }

            Logger.LogSeparator();

            return true;
        }
    }
}
