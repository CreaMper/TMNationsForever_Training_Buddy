using System.Collections.Generic;

namespace LogicStorage.Dtos.ApiRequests.TMXSearch
{

    public class APISearchDto
    {
        public bool More { get; set; }
        public List<APISearchResultDto> Results { get; set; }
    }

}