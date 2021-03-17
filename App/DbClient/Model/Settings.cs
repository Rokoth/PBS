using ProjectBranchSelector.DbClient.Model.Common;
using System;

namespace ProjectBranchSelector.DbClient
{
    public class Settings
    {
        public int Id { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
    }

    public class SyncConflict: Entity
    {
        public Guid SId { get; set; }        
        public string Table { get; set; }
        public DateTimeOffset ServerVersionDate { get; set; }        
        public bool ServerIsDeleted { get; set; }
        public DateTimeOffset LocalVersionDate { get; set; }
        public bool LocalIsDeleted { get; set; }
    }
}