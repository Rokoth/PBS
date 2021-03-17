using ProjectBranchSelector.DbClient.Model.Common;
using System;

namespace ProjectBranchSelector.DbClient.Context
{
    public class TreeItem : Entity
    {
        public string Name { get; set; }
        public Guid TreeId { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsLeaf { get; set; }
        public int SelectCount { get; set; }
        public int Weight { get; set; }
        public string AddFields { get; set; }
        public string Description { get; set; }
    }
}