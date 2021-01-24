using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

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

        /// <summary>
        /// Counts the number of lines before <paramref name="pos"/> in <paramref name="str"/>.
        /// </summary>
        /// <param name="str">the string to count the lines in</param>
        /// <param name="pos">the position in the string to count before</param>
        /// <returns>the number of lines before <paramref name="pos"/></returns>
        public static int CountLinesBefore(this string str, int pos)
        {
            char last = ' ';
            pos = Math.Min(Math.Max(pos, 0), str.Length);
            int count = 0;
            for (int i = 0; i < pos; i++)
            {
                var c = str[i];
                if (c == '\r' || (last != '\r' && c == '\n')) 
                    count++;
                last = c;
            }
            return count;
        }

        /// <summary>
        /// Finds the nearest line break before <paramref name="pos"/> in <paramref name="str"/>.
        /// </summary>
        /// <param name="str">the string to search in</param>
        /// <param name="pos">the position to find the nearest line break before</param>
        /// <returns>the position of the character immediately following the nearest line break before <paramref name="pos"/></returns>
        public static int FindLineBreakBefore(this string str, int pos)
        {
            pos = Math.Min(Math.Max(pos, 0), str.Length - 1);
            while (pos > 0)
            {
                var c = str[pos--];
                if (c == '\r' || c == '\n')
                    return pos + 2; // we return the char after the line break
            }
            return 0;
        }

        /// <summary>
        /// Finds the nearest line break after <paramref name="pos"/> in <paramref name="str"/>.
        /// </summary>
        /// <param name="str">the string to search in</param>
        /// <param name="pos">the position to find the nearest line break after</param>
        /// <returns>the position of the nearest line break after <paramref name="pos"/></returns>
        public static int FindLineBreakAfter(this string str, int pos)
        {
            pos = Math.Min(Math.Max(pos, 0), str.Length);
            while (pos < str.Length)
            {
                var c = str[pos++];
                if (c == '\r' || c == '\n')
                    return pos - 1; // because pos has already been incremented
            }
            return str.Length;
        }

        /// <summary>
        /// Creates a <see cref="ValueTuple{T1, T2}"/> of its arguments. Primarily for use with <see cref="Enumerable.Zip{TFirst, TSecond, TResult}(IEnumerable{TFirst}, IEnumerable{TSecond}, Func{TFirst, TSecond, TResult})"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first value.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="first">The first element.</param>
        /// <param name="second">The second element.</param>
        /// <returns>A <see cref="ValueTuple{T1, T2}"/> consisting of <paramref name="first"/> and <paramref name="second"/>.</returns>
        public static (T1 First, T2 Second) Tuple<T1, T2>(T1 first, T2 second) => (first, second);
    }
}
