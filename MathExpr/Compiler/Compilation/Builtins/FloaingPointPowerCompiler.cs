using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    public class FloaingPointPowerCompiler : ISpecialBinaryOperationCompiler
    {
        public bool TryCompile(Expression left, Expression right, out Expression result)
        {
            result = null!;
            if (left.Type != typeof(float) && left.Type != typeof(double)) return false;
            if (right.Type != typeof(float) && right.Type != typeof(double)) return false;

            var biggest = left.Type == typeof(double) ? typeof(double) : right.Type;

            var powMethod = Helpers.GetMethod<Action<double>>(a => Math.Pow(a, a))!;
            result = Expression.Call(powMethod,
                left.Type == typeof(float) ? Expression.Convert(left, typeof(double)) : left,
                right.Type == typeof(float) ? Expression.Convert(right, typeof(double)) : right);
            return true;
        }
    }
}
