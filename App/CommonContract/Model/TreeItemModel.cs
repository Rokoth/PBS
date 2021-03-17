using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{
    public class TreeItemModel: EntityModel
    {
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "TreeId")]
        [Required]
        public Guid TreeId { get; set; }

        [Display(Name = "ParentId")]
        [HiddenInput(DisplayValue = false)]
        public Guid? ParentId { get; set; }

        [Display(Name = "Наименование")]
        [Required]
        [Remote("CheckName", "TreeItem", AdditionalFields = "TreeId,ParentId", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool IsLeaf { get; set; }

        [Display(Name = "Вес")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage ="Вес должен быт задан и принимать положительное значение")]
        public int Weight { get; set; }

        [Display(Name = "Дополнительно")]       
        public string AddFields { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Родитель")]
        public TreeItemModel Parent { get; set; }

        [Display(Name = "Дерево")]
        public TreeModel Tree { get; set; }

        [Display(Name = "Количество выборов")]
        public int SelectCount { get; set; }
    }
}
