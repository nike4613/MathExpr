using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    /// <summary>
    /// The compiler for the builtin function <c>ln(x)</c> for .NET numeric types.
    /// </summary>
    public class BuiltinFunctionLn : IBuiltinFunction<object?>
    {
        /// <summary>
        /// The name of this builtin function as a constant value.
        /// </summary>
        public const string ConstName = "ln";
        /// <inheritdoc/>
        public string Name => ConstName;

        /// <inheritdoc/>
        public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationContext<object?> context, ITypeHintHandler typeHintHandler, [MaybeNullWhen(false)] out Expression expr)
        {
            if (arguments.Count != 1)
            {
                expr = null;
                return false;
            }

            var argExpr = context.Transform(arguments.First());

            if (argExpr.Type == typeof(float) || argExpr.Type == typeof(double))
            { // if this is a floating point
                var method = Helpers.GetMethod<Action<double>>(d => Math.Log(d))!;

                expr = Expression.Call(method, CompilerHelpers.ConvertToType(argExpr, typeof(double)));
                return true;
            }
            else if (CompilerHelpers.IsFloating(argExpr.Type) || CompilerHelpers.IsIntegral(argExpr.Type))
            { // if this is a built-in integer or decimal
                var method = Helpers.GetMethod<Action<decimal>>(d => DecimalMath.Ln(d))!;

                expr = Expression.Call(method, CompilerHelpers.ConvertToType(argExpr, typeof(decimal)));
                return true;
            }

            expr = null;
            return false;
        }
    }
}
