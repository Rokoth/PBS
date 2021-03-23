using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectBranchSelector.Models
{

    public class TreeModel: EntityModel
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Tree", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckFormulaId", "Tree", ErrorMessage = "FormulaId is not valid.")]
        [HiddenInput(DisplayValue = false)]
        public Guid FormulaId { get; set; }
        [Display(Name = "Формула")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Formula { get; set; }
    }

    public class TreeHistoryModel : EntityHistoryModel
    {
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [HiddenInput(DisplayValue = false)]
        public Guid FormulaId { get; set; }
    }

    public class TreeUpdater
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Tree", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckFormulaId", "Tree", ErrorMessage = "FormulaId is not valid.")]
        [HiddenInput(DisplayValue = false)]
        public Guid FormulaId { get; set; }
        public Guid Id { get; set; }
    }

    public class TreeCreator
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Tree", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckFormulaId", "Tree", ErrorMessage = "FormulaId is not valid.")]
        [HiddenInput(DisplayValue = false)]
        public Guid FormulaId { get; set; }
    }

    public class FormulaModel: EntityModel
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Formula", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }
        [Display(Name = "Формула")]
        public string Text { get; set; }
        [Display(Name = "По умолчанию")]
        public bool IsDefault { get; set; }
    }

    public class FormulaHistoryModel : EntityHistoryModel
    {
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Формула")]
        public string Text { get; set; }
        [Display(Name = "По умолчанию")]
        public bool IsDefault { get; set; }
    }

    public class FormulaCreator
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Formula", ErrorMessage = "Name is not valid.")]
        public string Name { get; set; }
        [Display(Name = "Формула")]
        public string Text { get; set; }
        [Display(Name = "По умолчанию")]
        public bool IsDefault { get; set; }
    }

    public class FormulaUpdater: FormulaCreator
    {
        public Guid Id { get; set; }
    }

    public class SelectRequest
    {
        public Guid TreeId { get; set; }
        public int Count { get; set; } = 1;
        public bool LeafOnly { get; set; } = true;
    }

    public class SelectRequestCustom
    {
        public IEnumerable<TreeItemCustom> Tree { get; set; }
        public string Formula { get; set; }
        public int Count { get; set; } = 1;
        public bool LeafOnly { get; set; } = true;
    }

    public class TreeItemCustom
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }        
        public string Fields { get; set; }
        public string Name { get; set; }
    }

    public class SelectResponse
    {
        public IEnumerable<SelectResponseElement> Result { get; set; }
    }

    public class SelectResponseElement
    {
	    public Guid Id { get; set; }
        public string Name { get; set; }
        public string[] FullPath { get; set; }
    }

}
