//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
using ProjectBranchSelector.Db.Model.Common;
using System;
using System.Linq.Expressions;

namespace ProjectBranchSelector.Db.Interface
{
    /// <summary>
    /// Обобщенный класс фильтра
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class Filter<TModel> where TModel : Entity
    {
        /// <summary>
        /// Номер страницы
        /// </summary>
        public int? Page { get; set; }
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// Поле сортировки
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// Выражение выбора элементов
        /// </summary>
        public Expression<Func<TModel, bool>> Selector { get; set; }
    }
}