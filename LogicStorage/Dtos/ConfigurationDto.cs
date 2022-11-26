using System;

namespace LogicStorage.Dtos
{
    [Serializable]
    public class ConfigurationDto
    {
        public bool ShowAllInterfaces { get; set; }
        public bool NetworkConfigured { get; set; }
        public bool ClientConfigured { get; set; }
        public int ClientPID { get; set; }
        public string NetworkInterfaceName { get; set; }
        public bool AllwaysPromptTextBox { get; set; }
        public bool MinimaliseExecutor { get; set; }
        public bool PreserveSettings { get; set; }
    }
}
