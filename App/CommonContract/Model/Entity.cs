using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace ProjectBranchSelector.Models
{
    public abstract class EntityModel
    {
        [Display(Name = "Идентификатор")]
        public Guid Id { get; set; }
        
    }

    public abstract class EntityHistoryModel: EntityModel
    {        
        public long HId { get; set; }        
        public DateTimeOffset ChangeDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
