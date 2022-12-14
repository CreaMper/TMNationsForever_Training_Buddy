using LogicStorage.Dtos;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LogicStorage.Utils
{
    public class Serializer
    {
        private readonly string _configFileName = "config.json";
        readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrainingBuddy");

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
    }
}
