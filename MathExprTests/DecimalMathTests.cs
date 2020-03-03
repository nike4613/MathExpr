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
        [InlineData(2, 2, 4, 0)]
        [InlineData(2, 3, 8, 0)]
        [InlineData(2, 4, 16, 0)]
        [InlineData(2, 16, ushort.MaxValue + 1, 0)]
        [InlineData(2, 19, 524288, 0)]
        [InlineData(6, 21, 21936950640377856UL, 0)]
        public void IntegerPower(long bas, long exp, long expect, int iters)
        {
            Assert.Equal((decimal)expect, DecimalMath.Pow(bas, exp, iters));
        }

        [Theory()] // args are decimal bits, being { lo, mid, hi, flags }
        // ln(1) = 0 (0 error)
        [InlineData(new[] { 1, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 })]
        // ln(2) = 0.69314718055994530941723212 (2e-27 error)
        [InlineData(new[] { 2, 0, 0, 0 }, new[] { unchecked((int)0x9DDD624C), 0x6523BB59, 0x3955F6, 0x001A0000 }, new[] { 2, 0, 0, 0x001A0000 })]
        // ln(e^2) = ln(7.389056098930650227230427460) = 2 (1.1688695634449e-14 error) (error is the actual max error of the approxamation)
        [InlineData(new[] { 0x307CC944, unchecked((int)0xC0FF3042), 0x17E0157D, 0x001B0000 }, new[] { 2, 0, 0, 0 }, new[] { 0xC8D1A11, 0xAA17, 0, 0x001b0000 })]
        // TODO: add more test cases
        public void NaturalLog(int[] arg, int[] expect, int[] error)
        {
            var logArg = new decimal(arg);
            var dexpect = new decimal(expect);
            var derror = new decimal(error);

            var actual = DecimalMath.Ln(logArg);
            var actualError = Math.Abs(dexpect - actual);
            Assert.True(derror >= Math.Abs(dexpect - actual), $"Error of {actualError}, expected no more than error of {derror}");
        }

        public void FractionalPower(long bas, int[] exp, int[] expect, int[] error, int iters, int logIters)
        {

        }

        // TODO: come up with a decent way to test fractional exponents, bases, and logs
    }
}
