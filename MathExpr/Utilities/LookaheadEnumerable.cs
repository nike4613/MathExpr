using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MathExpr.Utilities
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/> to allow easy creation of <see cref="LookaheadEnumerable{T}"/>s.
    /// </summary>
    public static class LookaheadEnumerable
    {
        /// <summary>
        /// Creates a <see cref="LookaheadEnumerable{T}"/> with the provided enumerable.
        /// </summary>
        /// <typeparam name="T">the element type</typeparam>
        /// <param name="e">the enumerable to wrap</param>
        /// <returns>a new <see cref="LookaheadEnumerable{T}"/> wrapping <paramref name="e"/></returns>
        public static LookaheadEnumerable<T> AsLookahead<T>(this IEnumerable<T> e)
            => new LookaheadEnumerable<T>(e);
        /// <summary>
        /// Creates a <see cref="LookaheadEnumerable{T}"/> with the provided enumerable and specified lookahead.
        /// </summary>
        /// <typeparam name="T">the element type</typeparam>
        /// <param name="e">the enumerable to wrap</param>
        /// <param name="lookahead">the amount of lookahead to use</param>
        /// <returns>a new <see cref="LookaheadEnumerable{T}"/> wrapping <paramref name="e"/></returns>
        public static LookaheadEnumerable<T> AsLookahead<T>(this IEnumerable<T> e, int lookahead)
            => new LookaheadEnumerable<T>(e, lookahead);
    }

    /// <summary>
    /// A special enumerable wrapper that keeps some amount of lookahead cached.
    /// </summary>
    /// <typeparam name="T">the element type</typeparam>
    public class LookaheadEnumerable<T>
    {
        private readonly IEnumerator<T> seq;

        private readonly T[] lookaheadArray;
        private int lookaheadStart = 0;
        private int lookaheadTail = 0;
        private int lookaheadLen = 0;

        private void QueueLookahead(T val)
        {
            if (lookaheadTail == lookaheadStart && lookaheadLen > 0)
                throw new InvalidOperationException("Max lookahead reached");
            lookaheadArray[lookaheadTail++] = val;
            lookaheadTail %= lookaheadArray.Length;
            lookaheadLen++;
        }
        private bool TryDequeueLookahead(out T val)
        {
            var remove = lookaheadLen >= 1;
            if (remove)
            {
                val = lookaheadArray[lookaheadStart];
                lookaheadArray[lookaheadStart++] = default!;
                lookaheadStart %= lookaheadArray.Length;
                lookaheadLen--;
            }
            else
                val = default!;
            return remove;
        }
        private T IndexLookahead(int idx)
            => lookaheadArray[(lookaheadStart + idx) % lookaheadArray.Length];

        /// <summary>
        /// Constructs a <see cref="LookaheadEnumerable{T}"/> with the given sequence and lookahead amount.
        /// </summary>
        /// <param name="seq">the sequence to iterate</param>
        /// <param name="lookahead">the amount of lookahead to use</param>
        public LookaheadEnumerable(IEnumerable<T> seq, int lookahead)
        {
            this.seq = seq.GetEnumerator();
            lookaheadArray = new T[lookahead];
        }
        /// <summary>
        /// Constructs a <see cref="LookaheadEnumerable{T}"/> with the given sequence and a default lookahead of 4.
        /// </summary>
        /// <param name="seq">the sequence to iterate</param>
        public LookaheadEnumerable(IEnumerable<T> seq) : this(seq, 4) { }

        /// <summary>
        /// Whether or not there is another element to read.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool HasNext => EnsureLookahead(1);

        private bool EnsureLookahead(int amount)
        {
            if (amount > lookaheadArray.Length)
                return false;

            if (lookaheadLen < amount)
            {
                var num = amount - lookaheadLen;
                for (int i = 0; i < num; i++)
                {
                    if (seq.MoveNext())
                        QueueLookahead(seq.Current);
                    else
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to look at the nth element after the current iteration position, if possible.
        /// </summary>
        /// <param name="value">the read value, if any</param>
        /// <param name="amount">the amount to ahead ahead</param>
        /// <returns><see langword="true"/> if an element was found, <see langword="false"/> otherwise</returns>
        public bool TryPeek([MaybeNullWhen(false)] out T value, int amount = 1)
        {
            if (amount <= 0) 
                throw new ArgumentException("Argument must be greater than 0");
            var res = EnsureLookahead(amount);
            if (res) value = IndexLookahead(amount - 1);
            else value = default!;
            return res;
        }

        /// <summary>
        /// Looks the specified amount ahead of the current iteration position.
        /// </summary>
        /// <param name="amount">the amount to look ahead</param>
        /// <returns>the element at that location</returns>
        /// <exception cref="InvalidOperationException">if there was no element at that position</exception>
        public T Peek(int amount = 1)
            => TryPeek(out var val, amount)
                ? val
                : throw new InvalidOperationException("Cannot peek past end of input enumerable");

        /// <summary>
        /// Attempts to get the next value and advance the iteration position.
        /// </summary>
        /// <param name="value">the next value, if any</param>
        /// <returns><see langword="true"/> if an element was retrieved, <see langword="false"/> otherwise</returns>
        public bool TryNext([MaybeNullWhen(false)] out T value)
        {
            var val = EnsureLookahead(1);
            if (val) val = TryDequeueLookahead(out value);
            else value = default!;
            return val;
        }

        /// <summary>
        /// Gets the next value and advances the iteration position.
        /// </summary>
        /// <returns>the next value</returns>
        /// <exception cref="InvalidOperationException">if there was no additional item</exception>
        public T Next()
            => TryNext(out var val) 
                ? val 
                : throw new InvalidOperationException("Cannot move past end of input enumerable");
    }
}
