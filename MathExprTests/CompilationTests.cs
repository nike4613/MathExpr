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
    }
}
