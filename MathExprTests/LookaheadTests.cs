using MathExpr.Utilities;
using MathExprTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class LookaheadTests
    {

        [Theory]
        [InlineData(0, 5, 1, 0, 0, false, true)]
        [InlineData(0, 5, 1, 1, 0, true, false)]
        [InlineData(0, 5, 2, 2, 1, true, false)]
        [InlineData(0, 5, 3, 3, 2, true, false)]
        [InlineData(0, 5, 4, 4, 3, true, false)]
        [InlineData(0, 5, 5, 5, 4, true, false)]
        [InlineData(0, 5, 6, 6, 5, true, false)]
        [InlineData(0, 5, 7, 7, 5, false, false)]
        [InlineData(0, 5, 5, 6, 5, false, false)]
        [InlineData(0, 5, 7, 7, -1, false, false)]
        [InlineData(1, 5, 1, 0, 1, false, true)]
        [InlineData(1, 5, 1, 1, 1, true, false)]
        [InlineData(1, 5, 2, 2, 2, true, false)]
        [InlineData(1, 5, 3, 3, 3, true, false)]
        [InlineData(1, 5, 4, 4, 4, true, false)]
        [InlineData(1, 5, 5, 5, 5, true, false)]
        [InlineData(1, 5, 5, 6, 5, false, false)]
        [InlineData(1, 5, 3, 4, 5, false, false)]
        [InlineData(1, 5, 6, 6, -1, false, false)]
        [InlineData(1, 5, 3, -3, 3, true, true)]
        public void SoloPeek(int start, int end, int lookaheadSize, int peekDist, int expect, bool valid, bool throws)
        {
            var lookahead = new RangeEnumerable(start..end).AsLookahead(lookaheadSize);

            try
            {
                Assert.Equal(valid, lookahead.TryPeek(out var val, peekDist));
                Assert.False(throws, "This is supposed to throw");
                if (valid)
                    Assert.Equal(expect, val);
            }
            catch (ArgumentException e)
            {
                Assert.True(throws, e.ToString());
            }
            catch (InvalidOperationException e)
            {
                Assert.True(throws, e.ToString());
            }
        }

        [Theory]
        [InlineData(0, 5, 1)]
        [InlineData(0, 5, 2)]
        [InlineData(0, 5, 3)]
        [InlineData(0, 5, 4)]
        [InlineData(0, 5, 5)]
        [InlineData(0, 5, 6)]
        [InlineData(0, 5, 7)]
        [InlineData(0, 5, 8)]
        [InlineData(0, 5, 9)]
        [InlineData(0, 5, 10)]
        [InlineData(1, 5, 1)]
        [InlineData(1, 5, 2)]
        [InlineData(1, 5, 3)]
        [InlineData(1, 5, 4)]
        [InlineData(1, 5, 5)]
        [InlineData(1, 5, 6)]
        [InlineData(1, 5, 7)]
        [InlineData(1, 5, 8)]
        [InlineData(1, 5, 9)]
        [InlineData(1, 5, 10)]
        public void AdvanceWithNext(int start, int end, int lookaheadSize)
        {
            var nums = new RangeEnumerable(start..end);
            var lookahead = nums.AsLookahead(lookaheadSize);

            foreach (var n in nums)
                Assert.Equal(n, lookahead.Next());
            Assert.False(lookahead.HasNext);
        }

        [Theory]
        [InlineData(0, 5, 1, true)]
        [InlineData(0, 5, 2, true)]
        [InlineData(0, 5, 3, true)]
        [InlineData(0, 5, 4, true)]
        [InlineData(0, 5, 5, true)]
        [InlineData(0, 5, 6, true)]
        [InlineData(0, 5, 7, false)]
        [InlineData(0, 5, 8, false)]
        [InlineData(0, 5, 9, false)]
        [InlineData(0, 5, 10, false)]
        [InlineData(1, 5, 1, true)]
        [InlineData(1, 5, 2, true)]
        [InlineData(1, 5, 3, true)]
        [InlineData(1, 5, 4, true)]
        [InlineData(1, 5, 5, true)]
        [InlineData(1, 5, 6, false)]
        [InlineData(1, 5, 7, false)]
        [InlineData(1, 5, 8, false)]
        [InlineData(1, 5, 9, false)]
        [InlineData(1, 5, 10, false)]
        public void AdvanceThroughLookahead(int start, int end, int lookaheadSize, bool peekSucceeds)
        {
            var nums = new RangeEnumerable(start..end);
            var lookahead = nums.AsLookahead(lookaheadSize);

            Assert.Equal(peekSucceeds, lookahead.TryPeek(out _, lookaheadSize));
            foreach (var n in nums)
                Assert.Equal(n, lookahead.Next());
            Assert.False(lookahead.HasNext);
        }

        [Theory]
        [InlineData(1, 10, 1, 1)]
        [InlineData(1, 10, 2, 1)]
        [InlineData(1, 10, 2, 2)]
        [InlineData(1, 10, 3, 1)]
        [InlineData(1, 10, 3, 2)]
        [InlineData(1, 10, 3, 3)]
        [InlineData(1, 10, 4, 1)]
        [InlineData(1, 10, 4, 2)]
        [InlineData(1, 10, 4, 3)]
        [InlineData(1, 10, 4, 4)]
        [InlineData(1, 10, 5, 1)]
        [InlineData(1, 10, 5, 2)]
        [InlineData(1, 10, 5, 3)]
        [InlineData(1, 10, 5, 4)]
        [InlineData(1, 10, 5, 5)]
        public void AdvanceThroughPartialLookahead(int start, int end, int lookaheadSize, int peekSize)
        {
            // tests lookahead array wrapping properly

            var nums = new RangeEnumerable(start..end);
            var lookahead = nums.AsLookahead(lookaheadSize);

            var count = 0;

            while (count < end-start+1)
            {
                lookahead.TryPeek(out _, peekSize);
                foreach (var n in nums.Skip(count).Take(peekSize))
                    Assert.Equal(n, lookahead.Next());
                count += peekSize;
            }
            Assert.False(lookahead.HasNext);
        }
    }
}
