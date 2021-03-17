using System.Collections.Generic;

namespace ProjectBranchSelector.Models
{
    public class TreeItemModelHuman : EntityModel
    {               
        public string Name { get; set; }        
        public int Weight { get; set; }
        public string AddFields { get; set; }        
        public string Description { get; set; }
        public int SelectCount { get; set; }
        public List<TreeItemModelHuman> Childs { get; set; }
    }
}
