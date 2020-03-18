using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace MathExpr.Utilities
{
    public static class Helpers
    {
        public static ulong IntegerFactorial(ulong val)
        {
            if (val == 0) return 1;
            var prod = val;
            while (--val > 0)
                prod *= val;
            return prod;
        }

        public static MethodInfo? GetMethod<TDel>(Expression<TDel> expr) where TDel : Delegate
            => (expr.Body as MethodCallExpression)?.Method;

        public static IEnumerable<T> Single<T>(T val)
        {
            yield return val;
        }
    }
}
