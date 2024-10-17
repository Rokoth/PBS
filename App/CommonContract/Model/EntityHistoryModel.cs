using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{
    public abstract class EntityHistoryModel: EntityModel
    {
        [Display(Name = "Идентификатор")]
        public long HId { get; set; }

        [Display(Name = "Дата изменения")]
        public DateTimeOffset ChangeDate { get; set; }

        [Display(Name = "Удаление")]
        public bool IsDeleted { get; set; }
    }
}
