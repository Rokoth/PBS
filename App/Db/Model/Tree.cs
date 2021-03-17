using Db.Attributes;
using ProjectBranchSelector.Db.Model.Common;
using System;

namespace ProjectBranchSelector.Db.Model
{
    [TableName("tree")]
    public class Tree : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }

        [ColumnName("formula_id")]
        public Guid FormulaId { get; set; }
    }

    [TableName("h_tree")]
    public class TreeHistory : EntityHistory
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }

        [ColumnName("formula_id")]
        public Guid FormulaId { get; set; }
    }

    public class RepositoryException : Exception 
    {
        public RepositoryException(string message) : base(message)
        { 
        
        }
    }
}
