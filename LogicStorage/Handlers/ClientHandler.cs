using LogicStorage.Dtos.TrackData;
using LogicStorage.Utils;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LogicStorage.Handlers
{
    public class ClientHandler
    {
        private static DLLImporter _importer =  new DLLImporter();
        
        public Process GetGameClientProcess()
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

        public bool VerifyClientFileStructure()
        {
            if (!Directory.Exists("GameData") || !Directory.Exists("Packs"))
                return false;

            if (!File.Exists("TmForever.exe") || !File.Exists("Nadeo.ini"))
                return false;

            return true;
        }

        public void InjectReplay(Process process, TrackStatsResultDto trackRecord)
        {
            _importer.UseSetForegroundWindow(process.MainWindowHandle);
            _importer.UseSetWindowText(process.MainWindowHandle, $"TM Training Buddy Client | Replay by {trackRecord.User.Name} | Time {trackRecord.ReplayTime}");

            var p = new Process();
            p.StartInfo = new ProcessStartInfo("replay.gbx")
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
