using System.Collections.Generic; 
namespace LogicStorage.Dtos.SearchQuery{ 

    public class SearchDto
    {
        public bool More { get; set; }
        public List<ResultSearchDto> Results { get; set; }
    }

}