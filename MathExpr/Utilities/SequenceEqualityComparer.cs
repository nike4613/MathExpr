using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MathExpr.Utilities
{
    public class SequenceEqualityComparer<T> : EqualityComparer<IEnumerable<T>>
    {
        private readonly IEqualityComparer<T> elementComparer;

        public static SequenceEqualityComparer<T> Default { get; } = new SequenceEqualityComparer<T>();

        public SequenceEqualityComparer() : this(EqualityComparer<T>.Default) { }
        public SequenceEqualityComparer(IEqualityComparer<T> elementComparer)
        {
            this.elementComparer = elementComparer;
        }

        public override bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return x.SequenceEqual(y, elementComparer);
        }

        public override int GetHashCode(IEnumerable<T> obj)
        {
            int code = unchecked((int)0xdeadbeef);
            foreach (var val in obj)
            {
                code ^= (code << 2) | (code >> 30);
                code ^= val == null ? 0 : elementComparer.GetHashCode(val);
            }
            return code;
        }
    }
}
