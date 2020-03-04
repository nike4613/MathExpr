using MathExpr.Compiler;
using MathExpr.Compiler.OptimizationPasses;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class OptimizationTests
    {
        /*[Theory]
        [MemberData(nameof(ReduceData))]
        public void Reduce(MathExpression toReduce, MathExpression expect)
        {
            var actual = toReduce.Reduce();
            Assert.Equal(expect, actual);
        }

        public static List<object[]> ReduceData = new List<object[]>
        {
            new object[] { ExpressionParser.ParseRoot("4 + 5"),     new LiteralExpression(9) },
            new object[] { ExpressionParser.ParseRoot("4 + 5 + 6"), new LiteralExpression(15) },
            new object[] { ExpressionParser.ParseRoot("9 + 5 + -4"),new LiteralExpression(10) },
            new object[] { ExpressionParser.ParseRoot("9 + 5 - 4"), new LiteralExpression(10) },
            new object[] { ExpressionParser.ParseRoot("5 > 4"),     new LiteralExpression(1) },
            new object[] { ExpressionParser.ParseRoot("5 < 4"),     new LiteralExpression(0) },
            new object[] { ExpressionParser.ParseRoot("4 * 5"),     new LiteralExpression(20) },
            new object[] { ExpressionParser.ParseRoot("4 * -5"),    new LiteralExpression(-20) },
            new object[] { ExpressionParser.ParseRoot("a + b + c"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { ExpressionParser.ParseRoot("a * b * c"), new BinaryExpression(BinaryExpression.ExpressionType.Multiply,
                new [] { new VariableExpression("c"), new VariableExpression("a"), new VariableExpression("b") }) },
            new object[] { ExpressionParser.ParseRoot("a - b - c"), new BinaryExpression(
                    new BinaryExpression(BinaryExpression.ExpressionType.Subtract,
                        new [] { new VariableExpression("a"), new VariableExpression("b") }), 
                    BinaryExpression.ExpressionType.Subtract,
                    new VariableExpression("c")) },
            new object[] { ExpressionParser.ParseRoot("1 + a + 2 + b"), new BinaryExpression(BinaryExpression.ExpressionType.Add,
                new MathExpression[] { new VariableExpression("b"), new VariableExpression("a"), new LiteralExpression(3) }) }
        };*/

        [Theory]
        [MemberData(nameof(BinaryExpressionCombinerPassTestData))]
        public void BinaryExpressionCombinerPass(MathExpression input, MathExpression expect)
        {
            var context = OptimizationContext.CreateWith(null, new[] { new BinaryExpressionCombinerPass() });

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
    }
}
