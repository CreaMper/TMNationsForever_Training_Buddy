using LogicStorage.Dtos;
using LogicStorage.Handlers;
using LogicStorage.Utils;
using SharpPcap;
using System;
using System.Diagnostics;
using System.Linq;

namespace Executor
{
    public static class RuntimeCheck
    {
        private static ExecutorConfigDto _config;
        private static Serializer _serializer;
        private static NetworkHandler _network;
        private static ClientHandler _client;
        private static RequestHandler _request;

        private static Process _clientProcess;
        private static ILiveDevice _device;
        private static string _initFailMsg = "";

        public static bool Check(ExecutorConfigDto config)
        {
            if (config == null)
            {
                Logger.Log("Cannot find configuration file, trying to start AUTO setup...", LogTypeEnum.Info);

                var deviceList = _network.GetDeviceList(false);
                var deviceName = deviceList.FirstOrDefault();
                if (deviceName == null)
                {
                    _initFailMsg = "Cannot find any suitable internet interface!";
                    return false;
                }
                var device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(deviceName));

                if (!_network.ChallangeInterface(device))
                {
                    _initFailMsg = "Interface Challange failed!";
                    return false;
                }

                var process = _client.GetGameClientProcess();
                if (process == null)
                {
                    _initFailMsg = "Cannot find a running Buddy client!";
                    return false;
                }

                Logger.Log("Program initialized successfully from AUTO configuration!", LogTypeEnum.Success);

                _device = device;
                _clientProcess = process;

                _config = new ExecutorConfigDto()
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
                    _clientProcess = Process.GetProcessById(_config.ClientPID);
                    if (_clientProcess.HasExited)
                    {
                        _initFailMsg = "It seems that you closed a Buddy Client!";
                        return false;
                    }

                    _device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(_config.NetworkInterfaceName));
                }
                catch
                {
                    _initFailMsg = "Configuration file data is obsolete/corrupted!";
                    return false;
                }

                Logger.Log("Program initialized successfully from a configuration file!", LogTypeEnum.Success);
            }

            Console.WriteLine();

            return true;
        }
    }
}
