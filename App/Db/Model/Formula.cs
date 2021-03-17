using Db.Attributes;
using ProjectBranchSelector.Db.Model.Common;

namespace ProjectBranchSelector.Db.Model
{
    [TableName("formula")]
    public class Formula : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("text")]
        public string Text { get; set; }

        [ColumnName("is_default")]
        public bool IsDefault { get; set; }
    }

    [TableName("h_formula")]
    public class FormulaHistory : EntityHistory
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("text")]
        public string Text { get; set; }

        [ColumnName("is_default")]
        public bool IsDefault { get; set; }
    }

    [TableName("settings")]
    public class Settings
    {
        [ColumnName("id")]
        public int Id { get; set; }
        [ColumnName("param_name")]
        public string ParamName { get; set; }
        [ColumnName("param_value")]
        public string ParamValue { get; set; }
    }
}
