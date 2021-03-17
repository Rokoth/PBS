using ProjectBranchSelector.DbClient.Context;
using ProjectBranchSelector.DbClient.Model.Common;
using System;

namespace ProjectBranchSelector.DbClient
{    
    public class Tree : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid FormulaId { get; set; }   
        
        public virtual Formula Formula { get; set; }
    }    
}