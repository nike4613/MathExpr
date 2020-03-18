using MathExpr.Compiler;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class EasyInterfaceTests
    {
        [Theory]
        [InlineData("x", 1.2d, 1.2d)]
        [InlineData("2*x", 1.5d, 3d)]
        [InlineData("2*x + 1", 1.5d, 4d)]
        public void CompileX(string expression, double arg, double expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<double, double>>(ExpressionParser.ParseRoot(expression), "x");
            Assert.Equal(expect, del(arg));
        }

        [Theory]
        [InlineData("x", 1.2d, 0d, 1.2d)]
        [InlineData("2*x", 1.5d, 0d, 3d)]
        [InlineData("2*x + 1", 1.5d, 0d, 4d)]
        [InlineData("x*y", 1.2d, 0d, 0d)]
        [InlineData("2*x*y", 1.5d, 0d, 0d)]
        [InlineData("(2*x + 1)*y", 1.5d, 0d, 0d)]
        [InlineData("x*y", 1.2d, 2d, 2.4d)]
        [InlineData("2*x*y", 1.5d, 2d, 6d)]
        [InlineData("(2*x + 1)*y", 1.5d, 2d, 8d)]
        public void CompileXY(string expression, double arg1, double arg2, double expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<double, double, double>>(ExpressionParser.ParseRoot(expression), "x", "y");
            Assert.Equal(expect, del(arg1, arg2));
        }
    }
}
