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
        [InlineData(0, 5, 5, 6, 5, false, true)]
        [InlineData(0, 5, 7, 7, -1, false, false)]
        [InlineData(1, 5, 1, 0, 1, false, true)]
        [InlineData(1, 5, 1, 1, 1, true, false)]
        [InlineData(1, 5, 2, 2, 2, true, false)]
        [InlineData(1, 5, 3, 3, 3, true, false)]
        [InlineData(1, 5, 4, 4, 4, true, false)]
        [InlineData(1, 5, 5, 5, 5, true, false)]
        [InlineData(1, 5, 5, 6, 5, false, false)]
        [InlineData(1, 5, 3, 4, 5, false, true)]
        [InlineData(1, 5, 6, 6, -1, false, false)]
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

        public void InterleavedNextPeek(int start, int end, int lookaheadSize, int cycleIters, int validCycles, int peeksBefore, int validPeeksBefore, int peeksAfter, int validPeeksAfter)
        {
            var list = new RangeEnumerable(start..end).ToArray();
            var lookahead = list.AsLookahead(lookaheadSize);

            int foundValidCycles = 0;
            foreach (var cycle in new RangeEnumerable(1..cycleIters))
            {
                int foundValidPeeksBefore = 0;
                foreach (var iBefore in new RangeEnumerable(1..peeksBefore))
                {
                    if (lookahead.TryPeek(out var val, iBefore))
                    {
                        foundValidPeeksBefore++;
                        Assert.Equal(list[cycle + iBefore - 2], val);
                    }
                }
                Assert.Equal(validPeeksBefore, foundValidPeeksBefore);

                if (lookahead.TryNext(out var val2))
                {
                    foundValidCycles++;
                    Assert.Equal(list[cycle - 1], val2);

                    int foundValidPeeksAfter = 0;
                }
            }
            
            // TODO: make this a sane test
        }
    }
}
