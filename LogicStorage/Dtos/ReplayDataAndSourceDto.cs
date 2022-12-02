using LogicStorage.Utils;

namespace LogicStorage.Dtos
{
    public class ReplayDataAndSourceDto
    {
        public string ReplayId { get; set; }
        public string Author { get; set; }
        public string Time { get; set; }
        public ApiTypeEnum Source { get; set; }
    }
}
