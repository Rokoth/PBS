using System;

namespace ProjectBranchSelector.DbClient.Model.Common
{
    public abstract class Entity
    {        
        public Guid Id { get; set; }       
        public DateTimeOffset VersionDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSync { get; set; }
    }

    public abstract class EntityHistory: Entity
    {
        public Guid HId { get; set; }       
        public DateTimeOffset ChangeDate { get; set; }
    }
}
