using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MathExpr.Utilities
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">the type in the sequence to compare</typeparam>
    public class SequenceEqualityComparer<T> : EqualityComparer<IEnumerable<T>>
    {
        private readonly IEqualityComparer<T> elementComparer;

        /// <summary>
        /// Gets the default sequence comparer for this type.
        /// </summary>
        public static new SequenceEqualityComparer<T> Default { get; } = new SequenceEqualityComparer<T>();

        /// <summary>
        /// Constructs a new <see cref="SequenceEqualityComparer{T}"/> using the default equality comparer for <typeparamref name="T"/>.
        /// </summary>
        public SequenceEqualityComparer() : this(EqualityComparer<T>.Default) { }
        /// <summary>
        /// Constructs a new <see cref="SequenceEqualityComparer{T}"/> using the provided equality comparer for the sequence elements.
        /// </summary>
        /// <param name="elementComparer">the <see cref="IEqualityComparer{T}"/> to use for the sequence elements</param>
        public SequenceEqualityComparer(IEqualityComparer<T> elementComparer)
        {
            this.elementComparer = elementComparer;
        }

        /// <inheritdoc/>
        public override bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return x.SequenceEqual(y, elementComparer);
        }

        /// <inheritdoc/>
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
