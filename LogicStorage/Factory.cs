using LogicStorage.Dtos;
using LogicStorage.Handlers;
using LogicStorage.Utils;

namespace LogicStorage
{
    public class Factory
    {
        public Factory()
        {
            _network = new NetworkHandler();
            _importer = new DLLImporter();
            _client = new ClientHandler(_importer);
            _request = new RequestHandler();
            _serializer = new Serializer();

            _executorConfig = _serializer.DeserializeExecutorConfig();
        }

        private readonly NetworkHandler _network;
        public NetworkHandler Network 
        { 
            get { return _network; } 
        }

        private readonly ClientHandler _client;
        public ClientHandler Client
        {
            get { return _client; }
        }

        private readonly RequestHandler _request;
        public RequestHandler Request
        {
            get { return _request; }
        }

        private readonly Serializer _serializer;
        public Serializer Serializer
        {
            get { return _serializer; }
        }

        private readonly DLLImporter _importer;
        public DLLImporter Importer
        {
            get { return _importer; }
        }

        private ExecutorConfigDto _executorConfig;
        public ExecutorConfigDto ExecutorConfig
        {
            get { return _executorConfig; }
            set { _executorConfig = value; }
        }
    }
}
