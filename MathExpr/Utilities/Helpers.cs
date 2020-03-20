using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace MathExpr.Utilities
{
    /// <summary>
    /// Various helper functions that are used by MathExpr.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Computes the factorial of an unsigned integral value.
        /// </summary>
        /// <param name="val">the factorial argument</param>
        /// <returns>the factorial of <paramref name="val"/></returns>
        public static ulong IntegerFactorial(ulong val)
        {
            if (val == 0) return 1;
            var prod = val;
            while (--val > 0)
                prod *= val;
            return prod;
        }

        /// <summary>
        /// Gets the method called as the outer expression of <paramref name="expr"/>.
        /// </summary>
        /// <example>
        /// This example will return the <see cref="MethodInfo"/> for <see cref="Math.Pow(double, double)"/>:
        /// <code>
        /// Helpers.GetMethod&lt;Action&lt;double&gt;&gt;(d =&gt; Math.Pow(d, d))
        /// </code>
        /// </example>
        /// <typeparam name="TDel">the delegate type of the parameter expression</typeparam>
        /// <param name="expr">the expression containing the method call</param>
        /// <returns>the <see cref="MethodInfo"/> of the call</returns>
        /// <seealso cref="GetConstructor{TDel}(Expression{TDel})"/>
        public static MethodInfo? GetMethod<TDel>(Expression<TDel> expr) where TDel : Delegate
            => (expr.Body as MethodCallExpression)?.Method;
        /// <summary>
        /// Gets the constructor called as the outer expression of <paramref name="expr"/>.
        /// </summary>
        /// <example>
        /// This example will return the <see cref="ConstructorInfo"/> for <see cref="OverflowException(string?)"/>:
        /// <code>
        /// Helpers.GetConstructor&lt;Action&lt;string&gt;&gt;(s =&gt; new OverflowException(s))
        /// </code>
        /// </example>
        /// <typeparam name="TDel">the delegate type of the parameter expression</typeparam>
        /// <param name="expr">the expression containing the constructor call</param>
        /// <returns>the <see cref="ConstructorInfo"/> of the call</returns>
        /// <seealso cref="GetMethod{TDel}(Expression{TDel})"/>
        public static ConstructorInfo? GetConstructor<TDel>(Expression<TDel> expr) where TDel : Delegate
            => (expr.Body as NewExpression)?.Constructor;

        /// <summary>
        /// Returns an enumerable containing only the single element <paramref name="val"/>.
        /// </summary>
        /// <typeparam name="T">the element type</typeparam>
        /// <param name="val">the single element of the enumerable</param>
        /// <returns>an enumerable containing only <paramref name="val"/></returns>
        public static IEnumerable<T> Single<T>(T val)
        {
            yield return val;
        }
    }
}
