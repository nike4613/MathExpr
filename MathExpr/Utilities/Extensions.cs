using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Utilities
{
    public static class Extensions
    {
        public static T PipeThrough<T, TElement>(this T start, IEnumerable<TElement> elements, Func<TElement, T, T> apply)
        {
            foreach (var el in elements)
                start = apply(el, start);
            return start;
        }
        public static T PipeThrough<T>(this T start, IEnumerable<Func<T, T>> funcs)
        {
            foreach (var f in funcs)
                start = f(start);
            return start;
        }
    }
}
