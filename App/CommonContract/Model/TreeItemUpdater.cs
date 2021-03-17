using System;
using System.Collections.Generic;

namespace ProjectBranchSelector.Models
{
    public class TreeItemUpdater
    {
        public Guid? Id { get; set; }        
        public string Name { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public string AddFields { get; set; }
        public IEnumerable<TreeItemUpdater> TreeItems { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TreeId { get; set; }
        public int SelectCount { get; set; }
    }
}
