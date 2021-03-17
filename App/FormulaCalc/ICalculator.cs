using System.Collections.Generic;

namespace FormulaCalc
{
    public interface ICalculator
    {
        IEnumerable<CalcResult> Calculate(CalcRequest request);
    }
}