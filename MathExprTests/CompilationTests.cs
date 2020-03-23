using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Compilation.Passes;
using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Linq;
using MathExpr.Utilities;

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
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var fn = Expression.Lambda<Func<object>>(Expression.Convert(
                    context.Transform(expr),
                    typeof(object)
                )).Compile();
            Assert.Equal(fn(), result);
        }

        public static readonly object[][] CompileLiteralTestValues = new[]
        {
            new object[] { MathExpression.Parse("15"), typeof(decimal), 15m },
            new object[] { MathExpression.Parse("15"), typeof(double), 15d },
            new object[] { MathExpression.Parse("15"), typeof(float), 15f },
            new object[] { MathExpression.Parse("15"), typeof(long), 15L },
            new object[] { MathExpression.Parse("15"), typeof(int), 15 },
        };

        [Theory]
        [MemberData(nameof(CompileUnaryTestValues))]
        public void CompileUnary(MathExpression expr, Type expectType, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var fn = Expression.Lambda<Func<object>>(Expression.Convert(
                    context.Transform(expr),
                    typeof(object)
                )).Compile();
            Assert.Equal(fn(), result);
        }

        public static readonly object[][] CompileUnaryTestValues = new[]
        {
            new object[] { MathExpression.Parse("-15"), typeof(int), -15 },
            new object[] { MathExpression.Parse("-(-15)"), typeof(int), 15 },
            new object[] { MathExpression.Parse("-15"), typeof(double), -15d },
            new object[] { MathExpression.Parse("-(-15)"), typeof(double), 15d },
            new object[] { MathExpression.Parse("~1"), typeof(int), 0 },
            new object[] { MathExpression.Parse("~2"), typeof(int), 0 },
            new object[] { MathExpression.Parse("~(-1)"), typeof(int), 0 },
            new object[] { MathExpression.Parse("~0"), typeof(int), 1 },
            new object[] { MathExpression.Parse("0!"), typeof(int), 1 },
            new object[] { MathExpression.Parse("1!"), typeof(int), 1 },
            new object[] { MathExpression.Parse("2!"), typeof(int), 2 },
            new object[] { MathExpression.Parse("3!"), typeof(int), 6 },
            new object[] { MathExpression.Parse("0!"), typeof(double), 1d },
            new object[] { MathExpression.Parse("1!"), typeof(double), 1d },
            new object[] { MathExpression.Parse("2!"), typeof(double), 2d },
            new object[] { MathExpression.Parse("3!"), typeof(double), 6d },
            new object[] { MathExpression.Parse("0!"), typeof(decimal), 1m },
            new object[] { MathExpression.Parse("1!"), typeof(decimal), 1m },
            new object[] { MathExpression.Parse("2!"), typeof(decimal), 2m },
            new object[] { MathExpression.Parse("3!"), typeof(decimal), 6m },
        };

        [Theory]
        [MemberData(nameof(CompileVariableTestValues))]
        public void CompileVariable(MathExpression expr, Type expectType, string paramName, object parameter, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var objParam = Expression.Parameter(typeof(object));
            var var = Expression.Variable(expectType);
            context.Settings.ParameterMap.Add(new VariableExpression(paramName), var);

            var fn = Expression.Lambda<Func<object, object>>(
                Expression.Block(
                    new[] { var },
                    Expression.Assign(var, Expression.Convert(objParam, expectType)),
                    Expression.Convert(
                        context.Transform(expr),
                        typeof(object)
                    )
                ), 
                objParam
            ).Compile();
            Assert.Equal(fn(parameter), result);
        }

        public static readonly object[][] CompileVariableTestValues = new[]
        {
            new object[] { MathExpression.Parse("x"), typeof(int), "x", 1, 1 },
            new object[] { MathExpression.Parse("x"), typeof(int), "x", 2, 2 },
            new object[] { MathExpression.Parse("x"), typeof(int), "x", 3, 3 },
            new object[] { MathExpression.Parse("-x"), typeof(int), "x", 1, -1 },
            new object[] { MathExpression.Parse("-x"), typeof(int), "x", 2, -2 },
            new object[] { MathExpression.Parse("-x"), typeof(int), "x", 3, -3 },
            new object[] { MathExpression.Parse("x!"), typeof(int), "x", 1, 1 },
            new object[] { MathExpression.Parse("x!"), typeof(int), "x", 2, 2 },
            new object[] { MathExpression.Parse("x!"), typeof(int), "x", 3, 6 },
            new object[] { MathExpression.Parse("abc"), typeof(int), "abc", 1, 1 },
            new object[] { MathExpression.Parse("abc"), typeof(int), "abc", 2, 2 },
            new object[] { MathExpression.Parse("abc"), typeof(int), "abc", 3, 3 },
            new object[] { MathExpression.Parse("-abc"), typeof(int), "abc", 1, -1 },
            new object[] { MathExpression.Parse("-abc"), typeof(int), "abc", 2, -2 },
            new object[] { MathExpression.Parse("-abc"), typeof(int), "abc", 3, -3 },
            new object[] { MathExpression.Parse("abc!"), typeof(int), "abc", 1, 1 },
            new object[] { MathExpression.Parse("abc!"), typeof(int), "abc", 2, 2 },
            new object[] { MathExpression.Parse("abc!"), typeof(int), "abc", 3, 6 },
        };

        [Theory]
        [MemberData(nameof(CompileBuiltinFunctionTestValues))]
        public void CompileBuiltinFunction(MathExpression expr, Type expectType, string paramName, object parameter, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var objParam = Expression.Parameter(typeof(object));
            var var = Expression.Variable(parameter.GetType());
            context.Settings.ParameterMap.Add(new VariableExpression(paramName), var);

            context.Settings.AddBuiltin().OfType<StringifyBuiltin>();

            var fn = Expression.Lambda<Func<object, object>>(
                Expression.Block(
                    new[] { var },
                    Expression.Assign(var, Expression.Convert(objParam, parameter.GetType())),
                    Expression.Convert(
                        context.Transform(expr),
                        typeof(object)
                    )
                ),
                objParam
            ).Compile();
            Assert.Equal(fn(parameter), result);
        }

        public static readonly object[][] CompileBuiltinFunctionTestValues = new[]
        {
            new object[] { MathExpression.Parse("toString(x)"), typeof(string), "x", 1, "hello 1" },
        };

        private class StringifyBuiltin : IBuiltinFunction<object?>
        {
            public string Name => "toString";
            public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationTransformContext<object?> context, ITypeHintHandler hinting, out Expression expr)
            {
                var arg = arguments.First();
                var concatMethod = Helpers.GetMethod<Action<string>>(a => string.Concat(a, a))!;
                expr = Expression.Call(typeof(Convert), nameof(Convert.ToString), null, context.Transform(arg));
                expr = Expression.Call(concatMethod, Expression.Constant("hello "), expr);
                return true;
            }
        }


        [Theory]
        [MemberData(nameof(CompileBinaryTestValues))]
        public void CompileBinary(MathExpression expr, Type expectType, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var fn = Expression.Lambda<Func<object>>(Expression.Convert(
                    context.Transform(expr),
                    typeof(object)
                )).Compile();
            Assert.Equal(fn(), result);
        }

        public static object[][] CompileBinaryTestValues = new[]
        {
            new object[] { MathExpression.Parse("4 + 5"),      typeof(int), 9 },
            new object[] { MathExpression.Parse("4 + 5 + 6"),  typeof(int), 15 },
            new object[] { MathExpression.Parse("9 + 5 + -4"), typeof(int), 10 },
            new object[] { MathExpression.Parse("9 + 5 - 4"),  typeof(int), 10 },
            new object[] { MathExpression.Parse("5 > 4"),      typeof(int), 1 },
            new object[] { MathExpression.Parse("5 < 4"),      typeof(int), 0 },
            new object[] { MathExpression.Parse("4 * 5"),      typeof(int), 20 },
            new object[] { MathExpression.Parse("4 * -5"),     typeof(int), -20 },
        };

        [Theory]
        [MemberData(nameof(CompileIfTestValues))]
        public void CompileIf(MathExpression expr, Type expectType, object xarg, object result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var objParam = Expression.Parameter(typeof(object));
            var var = Expression.Variable(expectType);
            context.Settings.ParameterMap.Add(new VariableExpression("x"), var);

            var fn = Expression.Lambda<Func<object, object>>(
                Expression.Block(
                    new[] { var },
                    Expression.Assign(var, Expression.Convert(objParam, expectType)),
                    Expression.Convert(
                        context.Transform(expr),
                        typeof(object)
                    )
                ),
                objParam
            ).Compile();
            Assert.Equal(fn(xarg), result);
        }

        public static object[][] CompileIfTestValues = new[]
        {
            new object[] { MathExpression.Parse("if(x > 5, 15, 25)"), typeof(int), 6, 15 },
            new object[] { MathExpression.Parse("if(x > 5, 15, 25)"), typeof(int), 4, 25 },
            new object[] { MathExpression.Parse("if(x > 5, x*2, x*3)"), typeof(int), 6, 12 },
            new object[] { MathExpression.Parse("if(x > 5, x*2, x*3)"), typeof(int), 5, 15 },
            new object[] { MathExpression.Parse("if(x > 5, x*2, x*3)"), typeof(int), 4, 12 },
            new object[] { MathExpression.Parse("if(x < 0.5, 8 * x^4, -8 * (x-1)^4 + 1)"), typeof(decimal), 0.7m, 0.9352m },
            new object[] { MathExpression.Parse("if(x < 0.5, 8 * x^4, -8 * (x-1)^4 + 1)"), typeof(decimal), 0.3m, 0.0648m },
        };

        [Theory]
        [MemberData(nameof(CompileDomainRestrictionTestValues))]
        public void CompileDomainRestriction(MathExpression expr, MathExpression restrict, Type expectType, object xarg, object result, bool shouldThrow)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings
            {
                ExpectReturn = expectType,
                IgnoreDomainRestrictions = false,
            }, new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var objParam = Expression.Parameter(typeof(object));
            var var = Expression.Variable(expectType);
            context.Settings.ParameterMap.Add(new VariableExpression("x"), var);

            context.Settings.DomainRestrictions.Add(restrict);

            var fn = Expression.Lambda<Func<object, object>>(
                Expression.Block(
                    new[] { var },
                    Expression.Assign(var, Expression.Convert(objParam, expectType)),
                    Expression.Convert(
                        context.Transform(expr),
                        typeof(object)
                    )
                ),
                objParam
            ).Compile();

            try
            {
                Assert.Equal(fn(xarg), result);
                Assert.False(shouldThrow, "Function should have thrown");
            }
            catch (Exception e)
            {
                Assert.True(shouldThrow, $"Function threw when it shouldn't have {e}");
            }
        }

        public static object[][] CompileDomainRestrictionTestValues = new[]
        {
            new object[] { MathExpression.Parse("1 / (2*x)"), MathExpression.Parse("x = 0"), typeof(decimal), 2m, 1m/4m, false },
            new object[] { MathExpression.Parse("1 / (2*x)"), MathExpression.Parse("x = 0"), typeof(decimal), 0m, 1m, true},
            new object[] { MathExpression.Parse("1 / (2*x + 1)"), MathExpression.Parse("2*x+1 = 0"), typeof(decimal), 2m, 1m/5m, false },
            new object[] { MathExpression.Parse("1 / (2*x + 1)"), MathExpression.Parse("2*x+1 = 0"), typeof(decimal), -1m/2m, 1m, true},
        };
    }
}
