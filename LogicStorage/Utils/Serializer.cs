using LogicStorage.Dtos;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LogicStorage.Utils
{
    public class Serializer
    {
        private string _configFileName = "config.json";

        public void SerializeExecutorConfig(ConfigurationDto configuration)
        {
            if (File.Exists(_configFileName))
                File.Delete(_configFileName);

            using var streamWriter = File.CreateText(_configFileName);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer.CreateDefault().Serialize(jsonWriter, configuration);
        }

        public ConfigurationDto DeserializeExecutorConfig()
        {
            if (!File.Exists(_configFileName))
                return null;

            try
            {
                var fileStream = File.ReadAllText(_configFileName);
                var configuration = JsonConvert.DeserializeObject<ConfigurationDto>(fileStream);
                return configuration;
            }
            catch
            {
                Console.WriteLine("Exception occured while deserialize config file!");
                return null;
            }
        }
    }
}
