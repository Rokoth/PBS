using System;

namespace ProjectBranchSelector.Contract
{
    public class TreeItemModel
    {
        public Guid Id { get; set; }
        public Guid TreeId { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public bool IsLeaf { get; set; }        
    }
}
