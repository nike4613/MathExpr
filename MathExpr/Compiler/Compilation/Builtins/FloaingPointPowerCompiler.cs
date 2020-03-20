using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    /// <summary>
    /// A compiler for the Pow operator for arguments that are <see cref="double"/> and <see cref="float"/>.
    /// </summary>
    public class FloaingPointPowerCompiler : ISpecialBinaryOperationCompiler
    {
        /// <summary>
        /// Attempts to compile the operation with the given arguments. This implementation is only successful
        /// when <c>left.Type</c> and <c>right.Type</c> are both either <see cref="double"/> or <see cref="float"/>.
        /// </summary>
        /// <param name="left">the base of the operation</param>
        /// <param name="right">the exponent of the operation</param>
        /// <param name="result">the result of the compilation</param>
        /// <returns><see langword="true"/> if it was compiled, <see langword="false"/> otherwise.</returns>
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
