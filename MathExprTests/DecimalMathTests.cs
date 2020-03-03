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

        #region Powers
        [Theory]
        [InlineData(2, 0, 1, 0)]
        [InlineData(2, 1, 2, 0)]
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
        #endregion

        #region Natural Log
        [Theory]
        [MemberData(nameof(NaturalLogTestValues))]
        public void NaturalLog(decimal arg, decimal expect, decimal error)
        {
            var actual = DecimalMath.Ln(arg);
            var actualError = Math.Abs(expect - actual);
            Assert.True(error >= actualError, $"Error of {actualError}, expected no more than error of {error}");
        }

        [Theory]
        [MemberData(nameof(NaturalLogThrowTestValues))]
        public void NaturalLogThrow(decimal arg)
            => Assert.Throws<OverflowException>(() => DecimalMath.Ln(arg));

        private const decimal LogMaxError = 1.1688695634449e-14m;
        public static object[][] NaturalLogTestValues = new[]
        {
            new object[] { 1m, 0m, 0m },
            new object[] { 2m, DecimalMath.Ln2, 0m },
            new object[] { 7.389056098930650227230427460m, 2m, LogMaxError },
            new object[] { 0.9m, -0.10536051565782630122750098083931m, LogMaxError },
            new object[] { 0.5m, -0.69314718055994530941723212145818m, LogMaxError },
            new object[] { 0.1m, -2.3025850929940456840179914546844m, LogMaxError },
            new object[] { 0.01m, -4.6051701859880913680359829093687m, LogMaxError },
        };
        public static object[][] NaturalLogThrowTestValues = new[]
        {
            new object[] { 0m },
            new object[] { -1m },
            new object[] { -.1m },
            new object[] { -10m },
        };
        #endregion
    }
}
