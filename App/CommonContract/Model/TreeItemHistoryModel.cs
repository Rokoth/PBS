using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{
    public class TreeItemHistoryModel : EntityHistoryModel
    {        
        [Display(Name = "TreeId")]       
        public Guid TreeId { get; set; }
        [Display(Name = "ParentId")]
        [HiddenInput(DisplayValue = false)]
        public Guid? ParentId { get; set; }
        [Display(Name = "Наименование")]        
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        public bool IsLeaf { get; set; }
        [Display(Name = "Вес")]
        public int Weight { get; set; }
        [Display(Name = "Дополнительно")]
        public string AddFields { get; set; }
        [Display(Name = "Количество выборов")]
        public int SelectCount { get; set; }
    }
}
