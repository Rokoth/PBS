using Db.Attributes;
using ProjectBranchSelector.Db.Model.Common;
using System;

namespace ProjectBranchSelector.Db.Model
{
    [TableName("tree_item")]
    public class TreeItem : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("description")]
        public string Description { get; set; }

        [ColumnName("tree_id")]
        public Guid TreeId { get; set; }

        [ColumnName("parent_id")]
        public Guid? ParentId { get; set; }

        [ColumnName("is_leaf")]
        public bool IsLeaf { get; set; }

        [ColumnName("select_count")]
        public int SelectCount { get; set; }

        [ColumnName("weight")]
        public int Weight { get; set; }

        [ColumnName("add_fields")]
        [ColumnType("json")]
        public string AddFields { get; set; }
    }

    [TableName("h_tree_item")]
    public class TreeItemHistory : EntityHistory
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }

        [ColumnName("tree_id")]
        public Guid TreeId { get; set; }

        [ColumnName("parent_id")]
        public Guid? ParentId { get; set; }

        [ColumnName("is_leaf")]
        public bool IsLeaf { get; set; }

        [ColumnName("select_count")]
        public int SelectCount { get; set; }

        [ColumnName("weight")]
        public int Weight { get; set; }

        [ColumnName("add_fields")]
        [ColumnType("json")]
        public string AddFields { get; set; }
    }
}
