using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    public class OtherNumericPowerCompiler : ISpecialBinaryOperationCompiler
    {
        private static bool IsIntegral(Type ty)
            => ty == typeof(long)
            || ty == typeof(ulong)
            || ty == typeof(int)
            || ty == typeof(uint)
            || ty == typeof(short)
            || ty == typeof(ushort)
            || ty == typeof(byte)
            || ty == typeof(sbyte);
        private static bool IsSigned(Type ty)
            => ty == typeof(long)
            || ty == typeof(int)
            || ty == typeof(short)
            || ty == typeof(sbyte);

        public bool TryCompile(Expression bas, Expression exp, out Expression result)
        {
            result = null!;

            var powMethod = Helpers.GetMethod<Action<decimal>>(a => DecimalMath.Pow(a, a))!;

            Type outType = typeof(decimal);
            if (IsIntegral(bas.Type) && IsIntegral(exp.Type) && !IsSigned(exp.Type)) 
                outType = IsSigned(bas.Type) ? typeof(long) : typeof(ulong);
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
