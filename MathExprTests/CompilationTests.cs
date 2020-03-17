using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Compilation.Passes;
using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq.Expressions;

namespace MathExprTests
{
    public class CompilationTests
    {
        [Theory]
        [MemberData(nameof(CompileLiteralTestValues))]
        public void CompileLiteral(MathExpression expr, Type expectType, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass());

            var fn = Expression.Lambda<Func<object>>(Expression.Convert(
                    context.Transform(expr),
                    typeof(object)
                )).Compile();
            Assert.Equal(fn(), result);
        }

        public static readonly object[][] CompileLiteralTestValues = new[]
        {
            new object[] { ExpressionParser.ParseRoot("15"), typeof(decimal), 15m },
            new object[] { ExpressionParser.ParseRoot("15"), typeof(double), 15d },
            new object[] { ExpressionParser.ParseRoot("15"), typeof(float), 15f },
            new object[] { ExpressionParser.ParseRoot("15"), typeof(long), 15L },
            new object[] { ExpressionParser.ParseRoot("15"), typeof(int), 15 },
        };

        [Theory]
        [MemberData(nameof(CompileUnaryTestValues))]
        public void CompileUnary(MathExpression expr, Type expectType, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass());

            var fn = Expression.Lambda<Func<object>>(Expression.Convert(
                    context.Transform(expr),
                    typeof(object)
                )).Compile();
            Assert.Equal(fn(), result);
        }

        public static readonly object[][] CompileUnaryTestValues = new[]
        {
            new object[] { ExpressionParser.ParseRoot("-15"), typeof(int), -15 },
            new object[] { ExpressionParser.ParseRoot("-(-15)"), typeof(int), 15 },
            new object[] { ExpressionParser.ParseRoot("-15"), typeof(double), -15d },
            new object[] { ExpressionParser.ParseRoot("-(-15)"), typeof(double), 15d },
            new object[] { ExpressionParser.ParseRoot("~1"), typeof(int), 0 },
            new object[] { ExpressionParser.ParseRoot("~2"), typeof(int), 0 },
            new object[] { ExpressionParser.ParseRoot("~(-1)"), typeof(int), 0 },
            new object[] { ExpressionParser.ParseRoot("~0"), typeof(int), 1 },
            new object[] { ExpressionParser.ParseRoot("0!"), typeof(int), 1 },
            new object[] { ExpressionParser.ParseRoot("1!"), typeof(int), 1 },
            new object[] { ExpressionParser.ParseRoot("2!"), typeof(int), 2 },
            new object[] { ExpressionParser.ParseRoot("3!"), typeof(int), 6 },
            new object[] { ExpressionParser.ParseRoot("0!"), typeof(double), 1d },
            new object[] { ExpressionParser.ParseRoot("1!"), typeof(double), 1d },
            new object[] { ExpressionParser.ParseRoot("2!"), typeof(double), 2d },
            new object[] { ExpressionParser.ParseRoot("3!"), typeof(double), 6d },
            new object[] { ExpressionParser.ParseRoot("0!"), typeof(decimal), 1m },
            new object[] { ExpressionParser.ParseRoot("1!"), typeof(decimal), 1m },
            new object[] { ExpressionParser.ParseRoot("2!"), typeof(decimal), 2m },
            new object[] { ExpressionParser.ParseRoot("3!"), typeof(decimal), 6m },
        };
    }
}
