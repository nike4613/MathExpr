using System;
using System.Collections.Generic;
using System.Text;
using MathExpr.Utilities;
using Xunit;

namespace MathExprTests.Utilities
{
    public class HelpersTests
    {
        [Theory]
        [InlineData(0L, 1L)]
        [InlineData(1L, 1L)]
        [InlineData(2L, 2L)]
        [InlineData(3L, 6L)]
        [InlineData(4L, 24L)]
        [InlineData(5L, 120L)]
        [InlineData(6L, 720L)]
        public void IntegerFactorial(ulong arg, ulong expect)
        {
            Assert.Equal(expect, Helpers.IntegerFactorial(arg));
        }

        [Theory]
        [InlineData("5\n6\n7", 0, 0)]
        [InlineData("5\n6\n7", 1, 0)]
        [InlineData("5\n6\n7", 2, 1)]
        [InlineData("5\n6\n7", 3, 1)]
        [InlineData("5\n6\n7", 4, 2)]
        public void CountLinesBefore(string str, int position, int expect)
        {
            Assert.Equal(expect, str.CountLinesBefore(position));
        }

        [Theory]
        [InlineData("5\n6\n7", 0, 0)]
        [InlineData("5\n6\n7", 1, 2)]
        [InlineData("5\n6\n7", 2, 2)]
        [InlineData("5\n6\n7", 3, 4)]
        [InlineData("5\n6\n7", 4, 4)]
        [InlineData("5\r\n6\r\n78", 0, 0)]
        [InlineData("5\r\n6\r\n78", 1, 2)]
        [InlineData("5\r\n6\r\n78", 2, 3)]
        [InlineData("5\r\n6\r\n78", 3, 3)]
        [InlineData("5\r\n6\r\n78", 4, 5)]
        [InlineData("5\r\n6\r\n78", 5, 6)]
        [InlineData("5\r\n6\r\n78", 6, 6)]
        [InlineData("5\r\n6\r\n78", 7, 6)]
        [InlineData("5\r\n6\r\n78", 8, 6)]
        public void FindLineBreakBefore(string str, int position, int expect)
        {
            Assert.Equal(expect, str.FindLineBreakBefore(position));
        }

        [Theory]
        [InlineData("5\n6\n7", 0, 1)]
        [InlineData("5\n6\n7", 1, 1)]
        [InlineData("5\n6\n7", 2, 3)]
        [InlineData("5\n6\n7", 3, 3)]
        [InlineData("5\n6\n7", 4, 5)]
        [InlineData("5\r\n6\r\n78", 0, 1)]
        [InlineData("5\r\n6\r\n78", 1, 1)]
        [InlineData("5\r\n6\r\n78", 2, 2)]
        [InlineData("5\r\n6\r\n78", 3, 4)]
        [InlineData("5\r\n6\r\n78", 4, 4)]
        [InlineData("5\r\n6\r\n78", 5, 5)]
        [InlineData("5\r\n6\r\n78", 6, 8)]
        [InlineData("5\r\n6\r\n78", 7, 8)]
        [InlineData("5\r\n6\r\n78", 8, 8)]
        public void FindLineBreakAfter(string str, int position, int expect)
        {
            Assert.Equal(expect, str.FindLineBreakAfter(position));
        }
    }
}
