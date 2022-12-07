using LogicStorage.Utils;
using System.Diagnostics;
using System.IO;

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

        private Process _user;
        public Process User
        {
            get { return _user; }
            set { _user = value; }
        }

        public bool VerifyClientFileStructure()
        {
            if (!Directory.Exists("GameData") || !Directory.Exists("Packs"))
                return false;

            if (!File.Exists("TmForever.exe") || !File.Exists("Nadeo.ini"))
                return false;

            return true;
        }

        public void InjectReplay(Process process)
        {
            _importer.UseSetForegroundWindow(process.MainWindowHandle);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo("replay.gbx")
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
