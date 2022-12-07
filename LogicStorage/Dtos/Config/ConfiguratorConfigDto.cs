using System;

namespace LogicStorage.Dtos.Config
{
    [Serializable]
    public class ConfiguratorConfigDto
    {
        public bool ShowAllInterfaces { get; set; }
        public bool NetworkConfigured { get; set; }
        public bool ClientConfigured { get; set; }
        public int ClientPID { get; set; }
        public string NetworkInterfaceName { get; set; }
        public int ListeningIntensivityLevel { get; set; }
        public bool MinimaliseExecutor { get; set; }
    }
}
