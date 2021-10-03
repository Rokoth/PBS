//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
using System.Collections.Generic;

namespace FormulaCalc
{
    /// <summary>
    /// Библиотека выбора элемента по формуле
    /// </summary>
    public interface ICalculator
    {
        /// <summary>
        /// Метод расчета по формуле
        /// </summary>
        /// <param name="request">Запрос расчета</param>
        /// <returns></returns>
        IEnumerable<CalcResult> Calculate(CalcRequest request);
    }
}