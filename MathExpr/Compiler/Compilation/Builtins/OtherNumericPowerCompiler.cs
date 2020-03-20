using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    /// <summary>
    /// A compiler for the Pow operator for arguments that are not <see cref="double"/> and <see cref="float"/>.
    /// </summary>
    public class OtherNumericPowerCompiler : ISpecialBinaryOperationCompiler
    {
        // TODO: incorporate hinting here
        /// <summary>
        /// Attempts to compile the operation with the given arguments.
        /// </summary>
        /// <param name="bas">the base of the operation</param>
        /// <param name="exp">the exponent of the operation</param>
        /// <param name="result">the result of the compilation</param>
        /// <returns><see langword="true"/> if it was compiled, <see langword="false"/> otherwise.</returns>
        public bool TryCompile(Expression bas, Expression exp, out Expression result)
        {
            result = null!;

            var powMethod = Helpers.GetMethod<Action<decimal>>(a => DecimalMath.Pow(a, a))!;

            Type outType = typeof(decimal);
            if (CompilerHelpers.IsIntegral(bas.Type) 
             && CompilerHelpers.IsIntegral(exp.Type) 
             && !CompilerHelpers.IsSigned(exp.Type)) 
                outType = CompilerHelpers.IsSigned(bas.Type) ? typeof(long) : typeof(ulong);
            try
            {
                if (bas.Type != typeof(decimal))
                    bas = Expression.Convert(bas, typeof(decimal));
                if (exp.Type != typeof(decimal))
                    exp = Expression.Convert(exp, typeof(decimal));
                result = Expression.Call(powMethod, bas, exp);
                if (result.Type != outType)
                    result = Expression.Convert(result, outType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
