using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NCalc2;
using Newtonsoft.Json.Linq;

namespace FormulaCalc
{
    public class CalculatorNCalc : ICalculator
    {

        public IEnumerable<CalcResult> Calculate(CalcRequest request)
        {
            List<CalcResult> result = new List<CalcResult>();

            for (int i = 0; i < request.Count; i++)
            {
                var res = CalculateInternal(request);
                if(res!=null)
                    request.ChangeOnSelect?.Invoke(res.Id);
                result.Add(res);
            }

            return result;
        }

        private CalcResult CalculateInternal(CalcRequest request)
        {
            //Regex regexMax = new Regex("^MAX\\((.*?)\\)$");
            //Regex regexMin = new Regex("^MIN\\((.*?)\\)$");
            //Regex regexAvg = new Regex("^AVG\\((.*?)\\)$");

            Guid? id = null;
            //if (regexMax.IsMatch(request.Formula))
            //{
            //    var formula = regexMax.Matches(request.Formula)[0].Groups[1].Value;
            //    id = CalculateMax(request, formula);
            //}
            //else if (regexMin.IsMatch(request.Formula))
            //{
            //    var formula = regexMin.Matches(request.Formula)[0].Groups[1].Value;
            //    id = CalculateMin(request, formula);
            //}
            //else if (regexAvg.IsMatch(request.Formula))
            //{
            //    var formula = regexAvg.Matches(request.Formula)[0].Groups[1].Value;
            //    id = CalculateAvg(request, formula);
            //}
            //else
            //{
            //    id = CalculateCommon(request);
            //}
            Expression expression = new Expression(request.Formula);
            expression.EvaluateFunction += NCalcExtensionFunctions;
            expression.EvaluateFunction += (name, args)=> NCalcExtensionFunctionGroup(name, args, request.Items);
            id = (Guid)expression.Evaluate();
            if (id.HasValue)
            {
                return new CalcResult(id.Value);
            }
            return null;
        }

        

        private Guid? CalculateMax(CalcRequest request, string formula)
        {
            Guid? result = null;
            int? maxValue = null;

            foreach (var item in request.Items)
            {
                Expression expression = new Expression(formula);
                expression.EvaluateFunction += NCalcExtensionFunctions;
                if (!string.IsNullOrEmpty(item.Fields))
                {
                    var fields = JObject.Parse(item.Fields)
                        .ToObject<Dictionary<string, object>>();
                    foreach (var field in fields)
                        expression.Parameters.Add(field.Key, field.Value);
                }
                var res = (int)expression.Evaluate();
                if (!maxValue.HasValue || res > maxValue)
                {
                    maxValue = res;
                    result = item.Id;
                }
            }
            return result;
        }

        private Guid? CalculateMin(CalcRequest request, string formula)
        {
            Guid? result = null;
            long? minValue = null;

            foreach (var item in request.Items)
            {
                Expression expression = new Expression(formula);
                expression.EvaluateFunction += NCalcExtensionFunctions;
                if (!string.IsNullOrEmpty(item.Fields))
                {
                    var fields = JObject.Parse(item.Fields)
                        .ToObject<Dictionary<string, object>>();
                    foreach (var field in fields)
                        expression.Parameters.Add(field.Key, field.Value);
                }
                var res = (long)expression.Evaluate();
                if (!minValue.HasValue || res < minValue)
                {
                    minValue = res;
                    result = item.Id;
                }
            }
            return result;
        }

        private Guid? CalculateAvg(CalcRequest request, string formula)
        {
            if (request.Items.Count() > 0)
            {
                Dictionary<Guid, int> values = new Dictionary<Guid, int>();
                var sum = 0m;
                foreach (var item in request.Items)
                {
                    Expression expression = new Expression(formula);
                    expression.EvaluateFunction += NCalcExtensionFunctions;
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            expression.Parameters.Add(field.Key, field.Value);
                    }
                    var res = (int)expression.Evaluate();
                    
                    values[item.Id] = res;
                    sum += res;
                }
                var avg = sum / request.Items.Count();
                decimal? minDelta = null;
                Guid? result = null;
                foreach (var item in values)
                {
                   var delta = Math.Abs(item.Value - avg);
                    if (!minDelta.HasValue || delta < minDelta)
                    {
                        minDelta = delta;
                        result = item.Key;
                    }
                }
                return result;
            }
            return null;
        }

        private Guid? CalculateCommon(CalcRequest request)
        {
            foreach (var item in request.Items)
            {
                Expression expression = new Expression(request.Formula);
                expression.EvaluateFunction += NCalcExtensionFunctions;
                
                if (!string.IsNullOrEmpty(item.Fields))
                {
                    var fields = JObject.Parse(item.Fields)
                        .ToObject<Dictionary<string, object>>();
                    foreach (var field in fields)
                        expression.Parameters.Add(field.Key, field.Value);
                }
                var res = (bool)expression.Evaluate();
                if (res)
                    return item.Id;
            }
            return null;
        }

        private void NCalcExtensionFunctionGroup(string name, FunctionArgs functionArgs, IEnumerable<CalcRequestItem> requestItems)
        {
            if (name == "MaxFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MaxFrom");
                
                long? maxValue = null;               
                foreach (var item in requestItems)
                {                    
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    if (!maxValue.HasValue || res > maxValue)
                    {
                        maxValue = res;
                    }
                }
                functionArgs.Result = maxValue;
            }
            if (name == "MinFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MinFrom");
                long? minValue = null;
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    if (!minValue.HasValue || res < minValue)
                    {
                        minValue = res;
                    }
                }
                functionArgs.Result = minValue;
            }
            if (name == "AvgFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове AvgFrom");
                long? sum = 0;
                Dictionary<Guid, long> values = new Dictionary<Guid, long>();
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    values[item.Id] = res;
                    sum += res;
                }
                var avg = sum / requestItems.Count();                
                functionArgs.Result = avg;
            }

            if (name == "MaxIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MaxIdFrom");
                long? maxValue = null;
                Guid? id = null;
                foreach (var item in requestItems)
                {                    
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    if (!maxValue.HasValue || res > maxValue)
                    {
                        id = item.Id;
                        maxValue = res;
                    }
                }
                functionArgs.Result = id;
            }
            if (name == "MinIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MinIdFrom");
                long? minValue = null;
                Guid? id = null;
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    if (!minValue.HasValue || res < minValue)
                    {
                        minValue = res;
                        id = item.Id;
                    }
                }
                functionArgs.Result = id;
            }
            if (name == "AvgIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове AvgIdFrom");
                long? sum = 0;
                Dictionary<Guid, long> values = new Dictionary<Guid, long>();
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (long)functionArgs.Parameters[0].Evaluate();
                    values[item.Id] = res;
                    sum += res;
                }
                var avg = sum / requestItems.Count();
                long? minDelta = null;
                Guid? result = null;
                foreach (var item in values)
                {
                    var delta = item.Value - avg;
                    if (!minDelta.HasValue || delta < minDelta)
                    {
                        minDelta = delta;
                        result = item.Key;
                    }
                }
                functionArgs.Result = result;
            }
        }

        private static void NCalcExtensionFunctions(string name, FunctionArgs functionArgs)
        {            
            if (name == "random")
            {
                if (functionArgs.Parameters.Count() == 0)
                {
                    functionArgs.Result = new Random().Next();
                }
                else if (functionArgs.Parameters.Count() == 1)
                {
                    functionArgs.Result = new Random().Next((int)(long)functionArgs.Parameters[0].Evaluate());
                }
                else
                {
                    functionArgs.Result = new Random().Next((int)(long)functionArgs.Parameters[0].Evaluate(), (int)(long)functionArgs.Parameters[1].Evaluate());
                }
            }
        }

    }

    public class CalcResult
    {
        public CalcResult(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }

    public class CalcRequest
    { 
        public IEnumerable<CalcRequestItem> Items { get; set; }
        public string Formula { get; set; }
        public int Count { get; set; }
        public Action<Guid> ChangeOnSelect { get; set; }
    }

    public class CalcRequestItem
    {
        public Guid Id { get; set; }
        public string Fields { get; set; }
    }

    
}

