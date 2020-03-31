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
using MathExpr.Compiler.Optimization.Settings;

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

            var restrictions = DomainRestrictionSettings.GetDomainRestrictionsFor(context);
            restrictions.Add(restrict);

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

        [Theory]
        [MemberData(nameof(CompileExpTestValues))]
        public void CompileExp(MathExpression expr, decimal xarg, decimal result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings(), 
                new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var var = Expression.Parameter(typeof(decimal));
            context.Settings.ParameterMap.Add(new VariableExpression("x"), var);

            var fn = Expression.Lambda<Func<decimal, decimal>>(
                context.Transform(expr),
                var
            ).Compile();

            var actual = fn(xarg);

            //Assert.True(Math.Abs(actual - result) < 1000 * DecimalMath.Epsilon, $"{actual} out of error margin of {result}");
            Assert.Equal(result, actual);
        }

        public static object[][] CompileExpTestValues = new[]
        {
            new object[] { MathExpression.Parse("exp(x)"), 0m, DecimalMath.Exp(0) },
            new object[] { MathExpression.Parse("exp(x)"), 1m, DecimalMath.Exp(1) },
            new object[] { MathExpression.Parse("exp(x)"), 2m, DecimalMath.Exp(2) },
            new object[] { MathExpression.Parse("exp(x)"), 3m, DecimalMath.Exp(3) },
            new object[] { MathExpression.Parse("exp(2*x)"), 0m, DecimalMath.Exp(0) },
            new object[] { MathExpression.Parse("exp(2*x)"), 1m, DecimalMath.Exp(2) },
            new object[] { MathExpression.Parse("exp(2*x)"), 2m, DecimalMath.Exp(4) },
            new object[] { MathExpression.Parse("exp(2*x)"), 3m, DecimalMath.Exp(6) },
            new object[] { MathExpression.Parse("exp(x/2)"), 0m, DecimalMath.Exp(0) },
            new object[] { MathExpression.Parse("exp(x/2)"), 2m, DecimalMath.Exp(1) },
            new object[] { MathExpression.Parse("exp(x/2)"), 4m, DecimalMath.Exp(2) },
            new object[] { MathExpression.Parse("exp(x/2)"), 6m, DecimalMath.Exp(3) },
        };

        [Theory]
        [MemberData(nameof(CompileLnTestValues))]
        public void CompileLn(MathExpression expr, decimal xarg, decimal result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings(),
                new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var var = Expression.Parameter(typeof(decimal));
            context.Settings.ParameterMap.Add(new VariableExpression("x"), var);

            var fn = Expression.Lambda<Func<decimal, decimal>>(
                context.Transform(expr),
                var
            ).Compile();

            var actual = fn(xarg);

            Assert.Equal(result, actual);
        }

        public static object[][] CompileLnTestValues = new[]
        {
            new object[] { MathExpression.Parse("ln(x)"), 1m, DecimalMath.Ln(1) },
            new object[] { MathExpression.Parse("ln(x)"), 2m, DecimalMath.Ln(2) },
            new object[] { MathExpression.Parse("ln(x)"), 3m, DecimalMath.Ln(3) },
            new object[] { MathExpression.Parse("ln(2*x)"), 1m, DecimalMath.Ln(2) },
            new object[] { MathExpression.Parse("ln(2*x)"), 2m, DecimalMath.Ln(4) },
            new object[] { MathExpression.Parse("ln(2*x)"), 3m, DecimalMath.Ln(6) },
            new object[] { MathExpression.Parse("ln(x/2)"), 1m, DecimalMath.Ln(1m/2m) },
            new object[] { MathExpression.Parse("ln(x/2)"), 2m, DecimalMath.Ln(1) },
            new object[] { MathExpression.Parse("ln(x/2)"), 4m, DecimalMath.Ln(2) },
            new object[] { MathExpression.Parse("ln(x/2)"), 6m, DecimalMath.Ln(3) },
        };

        [Theory]
        [MemberData(nameof(CompileTrigTestValues))]
        public void CompileTrig(MathExpression expr, double xarg, double result)
        {
            var context = CompilationTransformContext.CreateWith(new DefaultBasicCompileToLinqExpressionSettings()
                {
                    ExpectReturn = typeof(double)
                },
                new BasicCompileToLinqExpressionPass<DefaultBasicCompileToLinqExpressionSettings>());

            var var = Expression.Parameter(typeof(double));
            context.Settings.ParameterMap.Add(new VariableExpression("x"), var);

            var fn = Expression.Lambda<Func<double, double>>(
                context.Transform(expr),
                var
            ).Compile();

            var actual = fn(xarg);

            Assert.Equal(result, actual);
        }


        public static object[][] CompileTrigTestValues = new[]
        {
            new object[] { MathExpression.Parse("sin(x)"), 1d, Math.Sin(1) },
            new object[] { MathExpression.Parse("sin(x)"), 2d, Math.Sin(2) },
            new object[] { MathExpression.Parse("sin(2*x)"), 1d, Math.Sin(2) },
            new object[] { MathExpression.Parse("sin(2*x)"), 2d, Math.Sin(4) },
            new object[] { MathExpression.Parse("cos(x)"), 1d, Math.Cos(1) },
            new object[] { MathExpression.Parse("cos(x)"), 2d, Math.Cos(2) },
            new object[] { MathExpression.Parse("cos(2*x)"), 1d, Math.Cos(2) },
            new object[] { MathExpression.Parse("cos(2*x)"), 2d, Math.Cos(4) },
            new object[] { MathExpression.Parse("tan(x)"), 1d, Math.Tan(1) },
            new object[] { MathExpression.Parse("tan(x)"), 2d, Math.Tan(2) },
            new object[] { MathExpression.Parse("tan(2*x)"), 1d, Math.Tan(2) },
            new object[] { MathExpression.Parse("tan(2*x)"), 2d, Math.Tan(4) },
            new object[] { MathExpression.Parse("asin(x)"), 1d, Math.Asin(1) },
            new object[] { MathExpression.Parse("asin(x)"), 2d, Math.Asin(2) },
            new object[] { MathExpression.Parse("asin(2*x)"), 1d, Math.Asin(2) },
            new object[] { MathExpression.Parse("asin(2*x)"), 2d, Math.Asin(4) },
            new object[] { MathExpression.Parse("acos(x)"), 1d, Math.Acos(1) },
            new object[] { MathExpression.Parse("acos(x)"), 2d, Math.Acos(2) },
            new object[] { MathExpression.Parse("acos(2*x)"), 1d, Math.Acos(2) },
            new object[] { MathExpression.Parse("acos(2*x)"), 2d, Math.Acos(4) },
            new object[] { MathExpression.Parse("atan(x)"), 1d, Math.Atan(1) },
            new object[] { MathExpression.Parse("atan(x)"), 2d, Math.Atan(2) },
            new object[] { MathExpression.Parse("atan(2*x)"), 1d, Math.Atan(2) },
            new object[] { MathExpression.Parse("atan(2*x)"), 2d, Math.Atan(4) },
        };
    }
}
