using System.Collections.Generic; 
namespace Executor.Dtos{ 

    public class Root
    {
        public bool More { get; set; }
        public List<Result> Results { get; set; }
    }

}