using LogicStorage.Dtos;
using LogicStorage.Utils;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LogicStorage.Handlers
{
    public class ClientHandler
    {
        private readonly DLLImporter _importer;

        public ClientHandler(DLLImporter importer)
        {
            _importer = importer;
        }

        private Process _buddy;
        public Process Buddy
        {
            get { return _buddy; }
            set { _buddy = value; }
        }
        
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

        public void InjectReplay(Process process, ReplayDataAndSourceDto replay)
        {
            _importer.UseSetForegroundWindow(process.MainWindowHandle);
            _importer.UseSetWindowText(process.MainWindowHandle, $"TM Training Buddy Client | Replay by {replay.Author} | Time {replay.Time}");

            var p = new Process();
            p.StartInfo = new ProcessStartInfo("replay.gbx")
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
