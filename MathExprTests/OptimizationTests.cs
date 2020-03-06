using MathExpr.Compiler;
using MathExpr.Compiler.OptimizationPasses;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class OptimizationTests
    {
        // TODO: add tests for ICommutativitySettings.IgnoreCommutativityFor

        #region Basic Combiners
        [Theory]
        [MemberData(nameof(BinaryExpressionCombinerPassTestData))]
        public void BinaryExpressionCombinerPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings(), new BinaryExpressionCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static object[][] BinaryExpressionCombinerPassTestData = new []
        {
            new object[] { ExpressionParser.ParseRoot("a + b + c"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { ExpressionParser.ParseRoot("a * b * c"), new BinaryExpression(BinaryExpression.ExpressionType.Multiply,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { ExpressionParser.ParseRoot("a - b - c"), new BinaryExpression(
                    new BinaryExpression(BinaryExpression.ExpressionType.Subtract,
                        new [] { new VariableExpression("a"), new VariableExpression("b") }),
                    BinaryExpression.ExpressionType.Subtract,
                    new VariableExpression("c")) },
        };

        [Theory]
        [MemberData(nameof(LiteralCombinerPassTestData))]
        public void LiteralCombinerPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(null, new LiteralCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static object[][] LiteralCombinerPassTestData = new[]
        {
            new object[] { ExpressionParser.ParseRoot("4 + 5"),     new LiteralExpression(9) },
            new object[] { ExpressionParser.ParseRoot("4 + 5 + 6"), new LiteralExpression(15) },
            new object[] { ExpressionParser.ParseRoot("9 + 5 + -4"),new LiteralExpression(10) },
            new object[] { ExpressionParser.ParseRoot("9 + 5 - 4"), new LiteralExpression(10) },
            new object[] { ExpressionParser.ParseRoot("5 > 4"),     new LiteralExpression(1) },
            new object[] { ExpressionParser.ParseRoot("5 < 4"),     new LiteralExpression(0) },
            new object[] { ExpressionParser.ParseRoot("4 * 5"),     new LiteralExpression(20) },
            new object[] { ExpressionParser.ParseRoot("4 * -5"),    new LiteralExpression(-20) },
        };

        [Theory]
        [MemberData(nameof(BinaryCombinerLiteralCombinerPassesTestData))]
        public void BinaryCombinerLiteralCombinerPasses(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings(), new BinaryExpressionCombinerPass(), new LiteralCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static IEnumerable<object[]> BinaryCombinerLiteralCombinerPassesTestData =
            LiteralCombinerPassTestData.Concat(BinaryExpressionCombinerPassTestData)
                .Concat(new[]
                {
                    new object[] { ExpressionParser.ParseRoot("1 + a + 2 + b"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
                        new MathExpression[] { new VariableExpression("b"), new VariableExpression("a"), new LiteralExpression(3) }) }
                });
        #endregion

        #region Exponent Simplification
        [Theory]
        [MemberData(nameof(ExponentSimplificationPassTestData))]
        public void ExponentSimplificationPass(MathExpression input, MathExpression expect, MathExpression[] expectRestrictions, bool allowRestrictions)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings 
            { 
                AllowDomainChangingOptimizations = allowRestrictions 
            }, new ExponentSimplificationPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
            Assert.Equal(expectRestrictions.Length, context.Settings.DomainRestrictions.Count);
            foreach (var restrict in expectRestrictions)
                Assert.Contains(restrict, context.Settings.DomainRestrictions);
        }

        public static object[][] ExponentSimplificationPassTestData = new[]
        {
            new object[] { ExpressionParser.ParseRoot("exp(x)"), ExpressionParser.ParseRoot("exp(x)"), Array.Empty<MathExpression>(), true },
            new object[] { ExpressionParser.ParseRoot("exp(ln(x))"), ExpressionParser.ParseRoot("x"), new[] { ExpressionParser.ParseRoot("x <= 0") }, true },
            new object[] { ExpressionParser.ParseRoot("exp(ln(x - 2))"), ExpressionParser.ParseRoot("x - 2"), new[] { ExpressionParser.ParseRoot("x - 2 <= 0") }, true },
            new object[] { ExpressionParser.ParseRoot("exp(x)"), ExpressionParser.ParseRoot("exp(x)"), Array.Empty<MathExpression>(), false },
            new object[] { ExpressionParser.ParseRoot("exp(ln(x))"), ExpressionParser.ParseRoot("exp(ln(x))"), Array.Empty<MathExpression>(), false },
            new object[] { ExpressionParser.ParseRoot("exp(ln(x - 2))"), ExpressionParser.ParseRoot("exp(ln(x - 2))"), Array.Empty<MathExpression>(), false },
        };

        [Theory]
        [MemberData(nameof(ExponentSimplificationAndOtherPassesTestData))]
        public void ExponentSimplificationAndOtherPasses(MathExpression input, MathExpression expect, MathExpression[] expectRestrictions, bool allowRestrictions)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings
            {
                AllowDomainChangingOptimizations = allowRestrictions
            }, new ExponentSimplificationPass(), new BinaryExpressionCombinerPass(), new LiteralCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
            Assert.Equal(expectRestrictions.Length, context.Settings.DomainRestrictions.Count);
            foreach (var restrict in expectRestrictions)
                Assert.Contains(restrict, context.Settings.DomainRestrictions);
        }

        public static IEnumerable<object[]> ExponentSimplificationAndOtherPassesTestData =
            ExponentSimplificationPassTestData.Concat(new[]
            {
                new object[] { ExpressionParser.ParseRoot("exp(ln(x + 2)) + 5"), ExpressionParser.ParseRoot("x + 7"), new[] { ExpressionParser.ParseRoot("x + 2 <= 0") }, true },
                new object[] { ExpressionParser.ParseRoot("exp(ln(x + 2)) + 5"), ExpressionParser.ParseRoot("exp(ln(x + 2)) + 5"), Array.Empty<MathExpression>(), false },
            });
        #endregion

        #region Exponent Constant Reduction
        [Theory]
        [MemberData(nameof(ExponentConstantReductionPassTestData))]
        public void ExponentConstantReductionPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(null, new ExponentConstantReductionPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static object[][] ExponentConstantReductionPassTestData = new[]
        {
            new object[] { ExpressionParser.ParseRoot("exp(2)"), new LiteralExpression(DecimalMath.Exp(2)) },
            new object[] { ExpressionParser.ParseRoot("ln(2)"), new LiteralExpression(DecimalMath.Ln2) },
        };
        #endregion
    }
}
