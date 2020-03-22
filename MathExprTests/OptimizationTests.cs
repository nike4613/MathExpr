using MathExpr.Compiler.Optimization;
using MathExpr.Compiler.Optimization.Passes;
using MathExpr.Compiler.Optimization.Settings;
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
            new object[] { MathExpression.Parse("a + b + c"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { MathExpression.Parse("a * b * c"), new BinaryExpression(BinaryExpression.ExpressionType.Multiply,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { MathExpression.Parse("a - b - c"), new BinaryExpression(
                    new BinaryExpression(BinaryExpression.ExpressionType.Subtract,
                        new [] { new VariableExpression("a"), new VariableExpression("b") }),
                    new VariableExpression("c"),
                    BinaryExpression.ExpressionType.Subtract) },
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
            new object[] { MathExpression.Parse("4 + 5"),     new LiteralExpression(9) },
            new object[] { MathExpression.Parse("4 + 5 + 6"), new LiteralExpression(15) },
            new object[] { MathExpression.Parse("9 + 5 + -4"),new LiteralExpression(10) },
            new object[] { MathExpression.Parse("9 + 5 - 4"), new LiteralExpression(10) },
            new object[] { MathExpression.Parse("5 > 4"),     new LiteralExpression(1) },
            new object[] { MathExpression.Parse("5 < 4"),     new LiteralExpression(0) },
            new object[] { MathExpression.Parse("4 * 5"),     new LiteralExpression(20) },
            new object[] { MathExpression.Parse("4 * -5"),    new LiteralExpression(-20) },
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
                    new object[] { MathExpression.Parse("1 + a + 2 + b"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
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
            }, new BuiltinExponentSimplificationPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
            Assert.Equal(expectRestrictions.Length, context.Settings.DomainRestrictions.Count);
            foreach (var restrict in expectRestrictions)
                Assert.Contains(restrict, context.Settings.DomainRestrictions);
        }

        public static object[][] ExponentSimplificationPassTestData = new[]
        {
            new object[] { MathExpression.Parse("exp(x)"), MathExpression.Parse("exp(x)"), Array.Empty<MathExpression>(), true },
            new object[] { MathExpression.Parse("exp(ln(x))"), MathExpression.Parse("x"), new[] { MathExpression.Parse("x <= 0") }, true },
            new object[] { MathExpression.Parse("exp(ln(x - 2))"), MathExpression.Parse("x - 2"), new[] { MathExpression.Parse("x - 2 <= 0") }, true },
            new object[] { MathExpression.Parse("exp(x)"), MathExpression.Parse("exp(x)"), Array.Empty<MathExpression>(), false },
            new object[] { MathExpression.Parse("exp(ln(x))"), MathExpression.Parse("exp(ln(x))"), Array.Empty<MathExpression>(), false },
            new object[] { MathExpression.Parse("exp(ln(x - 2))"), MathExpression.Parse("exp(ln(x - 2))"), Array.Empty<MathExpression>(), false },
        };

        [Theory]
        [MemberData(nameof(ExponentSimplificationAndOtherPassesTestData))]
        public void ExponentSimplificationAndOtherPasses(MathExpression input, MathExpression expect, MathExpression[] expectRestrictions, bool allowRestrictions)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings
            {
                AllowDomainChangingOptimizations = allowRestrictions
            }, new BuiltinExponentSimplificationPass(), new BinaryExpressionCombinerPass(), new LiteralCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
            Assert.Equal(expectRestrictions.Length, context.Settings.DomainRestrictions.Count);
            foreach (var restrict in expectRestrictions)
                Assert.Contains(restrict, context.Settings.DomainRestrictions);
        }

        public static IEnumerable<object[]> ExponentSimplificationAndOtherPassesTestData =
            ExponentSimplificationPassTestData.Concat(new[]
            {
                new object[] { MathExpression.Parse("exp(ln(x + 2)) + 5"), MathExpression.Parse("x + 7"), new[] { MathExpression.Parse("x + 2 <= 0") }, true },
                new object[] { MathExpression.Parse("exp(ln(x + 2)) + 5"), MathExpression.Parse("exp(ln(x + 2)) + 5"), Array.Empty<MathExpression>(), false },
            });
        #endregion

        #region Exponent Constant Reduction
        [Theory]
        [MemberData(nameof(ExponentConstantReductionPassTestData))]
        public void ExponentConstantReductionPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(null, new BuiltinExponentConstantReductionPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static object[][] ExponentConstantReductionPassTestData = new[]
        {
            new object[] { MathExpression.Parse("exp(2)"), new LiteralExpression(DecimalMath.Exp(2)) },
            new object[] { MathExpression.Parse("ln(2)"), new LiteralExpression(DecimalMath.Ln2) },
        };
        #endregion

        #region Function Inlining
        [Theory]
        [MemberData(nameof(FunctionInliningPassTestData))]
        public void FunctionInliningPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(new DefaultOptimizationSettings(), 
                new UserFunctionInlinePass(), new LiteralCombinerPass());

            var actual = context.Optimize(input);
            Assert.Equal(expect, actual);
        }

        public static object[][] FunctionInliningPassTestData = new[]
        {
            new object[] { MathExpression.Parse("f'(x) = 2*x; 3*f'(2)"), new LiteralExpression(12) },
            new object[] { MathExpression.Parse("f'(x,y) = 2*x + x*y; 3 + f'(6, f'(2, 8))"), new LiteralExpression(135) },
            new object[] { MathExpression.Parse("a'(x) = x;" +
                "b'(x) = a'(a'(a'(a'(a'(a'(a'(x)))))));" +
                "c'(x) = b'(b'(b'(b'(b'(b'(b'(x)))))));" +
                "c'(x)"), new VariableExpression("x") }
        };
        #endregion
    }
}
