using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ProjectBranchSelector.Models
{
    public class TreeItemModelHuman : EntityModel
    {        
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Вес")]
        public int Weight { get; set; }

        [Display(Name = "Дополнительные поля")]
        public string AddFields { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Кол-во выборов")]
        public int SelectCount { get; set; }

        [Display(Name = "Дочерние элементы")]
        public List<TreeItemModelHuman> Childs { get; set; }
    }
}
