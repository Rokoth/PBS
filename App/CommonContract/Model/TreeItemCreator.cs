using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{
    public class TreeItemCreator
    {
        [Display(Name = "Идентификатор дерева")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckTreeId", "Tree", ErrorMessage = "Неверное дерево.")]
        public Guid TreeId { get; set; }
        
        [Display(Name = "Идентификатор родителя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckParentId", "Tree", ErrorMessage = "Неверный родитель.")]
        public Guid? ParentId { get; set; }

        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Tree", ErrorMessage = "Имя должно быть уникальным в ветке")]
        public string Name { get; set; }
        
        [Display(Name = "Описание")]
        public string Description { get; set; }
        
        [Display(Name = "Вес")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckWeight", "Tree", ErrorMessage = "Вес должен быть больше 0")]
        public int Weight { get; set; }

        [Display(Name = "Дополнительно")]
        public string AddFields { get; set; } 
    }
}
