using ProjectBranchSelector.DbClient.Model.Common;

namespace ProjectBranchSelector.DbClient.Context
{
    public class Formula : Entity
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public bool IsDefault { get; set; }
    }
}