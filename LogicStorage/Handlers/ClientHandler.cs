using LogicStorage.Utils;
using System.Diagnostics;
using System.Linq;

namespace LogicStorage.Handlers
{
    public class ClientHandler
    {
        private static DLLImporter _importer =  new DLLImporter();
        
        public Process GetProcessByName()
        {
            var legacy = Process.GetProcessesByName(System.IO.Path.GetFileName("TM Training Buddy Client")).FirstOrDefault();
            if (legacy != null)
                return legacy;

            var standaloneClient = Process.GetProcessesByName(System.IO.Path.GetFileName("TmForever")).FirstOrDefault();
            if (standaloneClient != null)
                return standaloneClient;

            var process = Process.GetProcessesByName(System.IO.Path.GetFileName("TrackMania Nations Forever")).FirstOrDefault();
            if (process != null)
                return process;

            return null;
        }
    }
}
