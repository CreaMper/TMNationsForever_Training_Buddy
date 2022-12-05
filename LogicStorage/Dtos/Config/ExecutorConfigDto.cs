using System;

namespace LogicStorage.Dtos.Config
{
    [Serializable]
    public class ExecutorConfigDto
    {
        public int ClientPID { get; set; }
        public string NetworkInterfaceName { get; set; }
        public int ListeningIntensivityLevel { get; set; }
        public bool MinimaliseExecutor { get; set; }

        public int ListeningIntensivityMiliseconds
        {
            get { return 1100 - 100 * ListeningIntensivityLevel; }
        }
    }
}
