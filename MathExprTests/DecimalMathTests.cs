using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class DecimalMathTests
    {
        [Theory]
        [InlineData(0L, 1L)]
        [InlineData(1L, 1L)]
        [InlineData(2L, 2L)]
        [InlineData(3L, 6L)]
        [InlineData(4L, 24L)]
        [InlineData(5L, 120L)]
        [InlineData(6L, 720L)]
        public void Factorial(long arg, long expect)
        {
            Assert.Equal((decimal)expect, DecimalMath.Factorial(arg));
        }

        [Theory]
        [InlineData(2, 2, 4, 0, 0)]
        [InlineData(2, 3, 8, 0, 0)]
        [InlineData(2, 4, 16, 0, 0)]
        [InlineData(2, 16, ushort.MaxValue + 1, 0, 0)]
        [InlineData(2, 19, 524288, 0, 0)]
        [InlineData(6, 21, 21936950640377856UL, 0, 0)]
        public void IntegerPower(long bas, long exp, long expect, int iters, int logIters)
        {
            Assert.Equal((decimal)expect, DecimalMath.Pow(bas, exp, iters, logIters));
        }

        // TODO: come up with a decent way to test fractional exponents, bases, and logs
    }
}
