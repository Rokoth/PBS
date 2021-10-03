//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
using ProjectBranchSelector.DbClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectBranchSelector.DbClient.Interface
{
    /// <summary>
    /// Методы работы с базой данных
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface IRepository<TModel> where TModel : Entity
    {
        /// <summary>
        /// Добавление модели
        /// </summary>
        /// <param name="entity">Модель</param>
        /// <param name="withSave">Сохранить в базе после добавления</param>
        /// <returns></returns>
        TModel Add(TModel entity, bool withSave = false);
        /// <summary>
        /// Получить модель по id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withDeleted">Искать в том числе в удаленных</param>
        /// <returns></returns>
        TModel Get(Guid id, bool withDeleted = false);
        /// <summary>
        /// Получить все модели
        /// </summary>
        /// <param name="withDeleted">Искать в том числе в удаленных</param>
        /// <returns></returns>
        IQueryable<TModel> GetAll(bool withDeleted = false);
        /// <summary>
        /// Удаление модели по id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withSave">Сохранить в базе изменения</param>
        /// <returns></returns>
        TModel Remove(Guid id, bool withSave = false);
        /// <summary>
        /// Обновление модели
        /// </summary>
        /// <param name="entity">Модель</param>
        /// <param name="withSave">Сохранить в базе изменения</param>
        /// <returns></returns>
        TModel Update(TModel entity, bool withSave = false);
        /// <summary>
        /// Получить модели по фильтру
        /// </summary>
        /// <param name="filterr">Фильтр</param>
        /// <param name="withDeleted">Искать в том числе в удаленных</param>
        /// <returns></returns>
        IEnumerable<TModel> Get(Filter<TModel> filterr, bool withDeleted = false);
    }
}