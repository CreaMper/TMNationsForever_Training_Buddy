using LogicStorage.Dtos.Config;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LogicStorage.Utils
{
    public class Serializer
    {
        private string _configFileName = "config.json";
        string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrainingBuddy");

        public void SerializeExecutorConfig(ConfiguratorConfigDto configurator)
        {
            var executorConfiguration = Converter(configurator);

            if (File.Exists(_configFileName))
                File.Delete(_configFileName);

            using var streamWriter = File.CreateText(_configFileName);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer.CreateDefault().Serialize(jsonWriter, executorConfiguration);
        }

        public void SerializeExecutorConfig(ExecutorConfigDto configurator)
        {
            if (File.Exists(_configFileName))
                File.Delete(_configFileName);

            using var streamWriter = File.CreateText(_configFileName);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer.CreateDefault().Serialize(jsonWriter, configurator);
        }

        public void SerializeConfiguratorConfig(ConfiguratorConfigDto configurator)
        {
            if (File.Exists(_configFileName))
                File.Delete(_configFileName);

            using var streamWriter = File.CreateText(_configFileName);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer.CreateDefault().Serialize(jsonWriter, configurator);
        }

        public bool RemoveCorruptedBuddyConfig()
        {
            try
            {
                var file = Path.Combine(_configPath, _configFileName);

                if (File.Exists(file))
                    File.Delete(file);
            
                return true;
            } 
            catch 
            {
                return false;
            }
        }

        public ExecutorConfigDto DeserializeExecutorConfig()
        {
            if (!File.Exists(_configFileName))
                return null;

            try
            {
                var fileStream = File.ReadAllText(_configFileName);
                var configuration = JsonConvert.DeserializeObject<ExecutorConfigDto>(fileStream);
                return configuration;
            }
            catch
            {
                Console.WriteLine("Exception occured while deserialize config file!");
                return null;
            }
        }

        public BuddyConfigDto DeserializeBuddyConfig()
        {
            if (!Directory.Exists(_configPath))
                return null;

            if (!File.Exists(Path.Combine(_configPath, _configFileName)))
                return null;

            try
            {
                var fileStream = File.ReadAllText(Path.Combine(_configPath, _configFileName));
                var configuration = JsonConvert.DeserializeObject<BuddyConfigDto>(fileStream);
                return configuration;
            }
            catch
            {
                return null;
            }
        }

        public void SerializeBuddyConfig(BuddyConfigDto config)
        {
            var file = Path.Combine(_configPath, _configFileName);

            if (!Directory.Exists(_configPath))
                Directory.CreateDirectory(_configPath);

            if (File.Exists(file))
                File.Delete(file);

            using var streamWriter = File.CreateText(file);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer.CreateDefault().Serialize(jsonWriter, config);
        }

        private ExecutorConfigDto Converter(ConfiguratorConfigDto configurator)
        {
            return new ExecutorConfigDto()
            {
                ClientPID = configurator.ClientPID,
                NetworkInterfaceName = configurator.NetworkInterfaceName,
                ListeningIntensivityLevel = configurator.ListeningIntensivityLevel,
                MinimaliseExecutor = configurator.MinimaliseExecutor
            };
        }
    }
}
