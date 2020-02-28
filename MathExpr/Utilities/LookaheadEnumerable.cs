﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MathExpr.Utilities
{
    public static class LookaheadEnumerable
    {
        public static LookaheadEnumerable<T> AsLookahead<T>(this IEnumerable<T> e)
            => new LookaheadEnumerable<T>(e);
        public static LookaheadEnumerable<T> AsLookahead<T>(this IEnumerable<T> e, int lookahead)
            => new LookaheadEnumerable<T>(e, lookahead);
    }

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

        public LookaheadEnumerable(IEnumerable<T> seq, int lookahead)
        {
            this.seq = seq.GetEnumerator();
            lookaheadArray = new T[lookahead];
        }
        // defaults to max lookahead of 2
        public LookaheadEnumerable(IEnumerable<T> seq) : this(seq, 4) { }

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

        public bool TryPeek(out T value, int amount = 1)
        {
            if (amount <= 0) 
                throw new ArgumentException("Argument must be greater than 0");
            var res = EnsureLookahead(amount);
            if (res) value = IndexLookahead(amount - 1);
            else value = default!;
            return res;
        }

        public T Peek(int amount = 1)
            => TryPeek(out var val, amount)
                ? val
                : throw new InvalidOperationException("Cannot peek past end of input enumerable");

        public bool TryNext(out T value)
        {
            var val = EnsureLookahead(1);
            if (val) val = TryDequeueLookahead(out value);
            else value = default!;
            return val;
        }

        public T Next()
            => TryNext(out var val) 
                ? val 
                : throw new InvalidOperationException("Cannot move past end of input enumerable");
    }
}
