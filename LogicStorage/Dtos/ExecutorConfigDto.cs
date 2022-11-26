using System;

namespace LogicStorage.Dtos
{
    [Serializable]
    public class ExecutorConfigDto
    {
        public int ClientPID { get; set; }
        public string NetworkInterfaceName { get; set; }
        public int ListeningIntensivityLevel { get; set; }
        public bool MinimaliseExecutor { get; set; }
    }
}
