using LogicStorage.Utils;

namespace LogicStorage.Dtos.ReplayList
{
    public class ReplayDto
    {
        public int ReplayId { get; set; }
        public ApiTypeEnum Source { get; set; }
        public string Player { get; set; }
        public string Time { get; set; }
        public int Rank { get; set; }
    }
}
