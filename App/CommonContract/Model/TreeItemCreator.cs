using System;

namespace ProjectBranchSelector.Models
{
    public class TreeItemCreator
    {
        public Guid TreeId { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public string AddFields { get; set; }
        public int SelectCount { get; set; }

    }
}
