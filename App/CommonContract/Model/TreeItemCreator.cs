using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{
    public class TreeItemCreator
    {
        [Display(Name = "Идентификатор дерева")]
        public Guid TreeId { get; set; }
        [Display(Name = "Идентификатор родителя")]
        public Guid? ParentId { get; set; }
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Вес")]
        public int Weight { get; set; }
        [Display(Name = "Дополнительно")]
        public string AddFields { get; set; }
        [Display(Name = "Количество выборов")]
        public int SelectCount { get; set; }

    }
}
