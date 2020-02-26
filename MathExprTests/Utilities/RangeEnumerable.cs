using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MathExprTests.Utilities
{
    public struct RangeEnumerable : IEnumerable<int>
    {
        public Range Range { get; }
        public RangeEnumerable(Range range)
            => Range = range;

        public struct Enumerator : IEnumerator<int>
        {
            private readonly int start;
            private readonly int end;
            
            public int Current { get; private set; }

            object? IEnumerator.Current => Current;

            internal Enumerator(Range range)
            {
                var (begin, len) = range.GetOffsetAndLength(int.MaxValue);
                start = begin;
                end = begin + len;
                Current = start - 1;
            }

            public bool MoveNext()
                => (++Current) <= end;

            public void Reset()
                => Current = start - 1;

            public void Dispose() { }
        }

        public Enumerator GetEnumerator()
            => new Enumerator(Range);

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override bool Equals(object? obj)
            => obj is RangeEnumerable e && Range.Equals(e.Range);

        public override int GetHashCode()
            => HashCode.Combine(Range);

        public static bool operator ==(RangeEnumerable left, RangeEnumerable right)
            => left.Equals(right);

        public static bool operator !=(RangeEnumerable left, RangeEnumerable right)
            => !(left == right);
    }
}
