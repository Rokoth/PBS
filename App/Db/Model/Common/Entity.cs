using Db.Attributes;
using System;

namespace ProjectBranchSelector.Db.Model.Common
{
    public abstract class Entity
    {
        [ColumnName("id")]
        [PrimaryKey]
        public virtual Guid Id { get; set; }
        
        [ColumnName("version_date")]
        public DateTimeOffset VersionDate { get; set; }

        [ColumnName("is_deleted")]
        public bool IsDeleted { get; set; }
    }

    public abstract class EntityHistory: Entity
    {
        [ColumnName("id")]
        public override Guid Id { get; set; }

        [ColumnName("h_id")]
        [PrimaryKey]
        public long HId { get; set; }

        [ColumnName("change_date")]
        public DateTimeOffset ChangeDate { get; set; }
    }
 }
