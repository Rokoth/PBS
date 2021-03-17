using System;
using System.Collections.Generic;

namespace ProjectBranchSelector.Models
{
    public class TreeItemsUpdater
    {
        public Guid TreeId { get; set; }
        public IEnumerable<TreeItemUpdater> TreeItems { get; set; }
    }
}
