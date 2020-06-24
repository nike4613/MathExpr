using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    /// <summary>
    /// The compiler for the builtin function <c>if(condition, ifTrue, ifFalse)</c>.
    /// </summary>
    /// <remarks>
    /// The condition will be evaluated as a boolean-like value, where a nonzero value is
    /// considered <see langword="true"/>, and zero is considered <see langword="false"/>.
    /// </remarks>
    public class BuiltinFunctionIf : IBuiltinFunction<object?>
    {
        /// <summary>
        /// The name of this builtin function as a constant value.
        /// </summary>
        public const string ConstName = "if";
        /// <inheritdoc/>
        public string Name => ConstName;

        /// <inheritdoc/>
        public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationContext<object?> ctx, ITypeHintHandler hintHandler, [MaybeNullWhen(false)] out Expression expr)
        {
            if (arguments.Count != 3)
            {
                expr = null;
                return false;
            }

            var condition = hintHandler.TransformWithHint(arguments.First(), typeof(bool), ctx);

            var hint = hintHandler.CurrentHint(ctx);
            Expression? thenExpr = null, elseExpr = null;
            Type? convertAllTo, lastThen, lastElse;
            do
            {
                lastThen = thenExpr?.Type;
                lastElse = elseExpr?.Type;
                thenExpr = hintHandler.TransformWithHint(arguments[1], hint, ctx);
                elseExpr = hintHandler.TransformWithHint(arguments[2], hint, ctx);

                var allTypes = new[] { thenExpr.Type, elseExpr.Type };
                convertAllTo = allTypes
                    .OrderByDescending(CompilerHelpers.EstimateTypeSize)
                    .Where(ty => allTypes.All(t => CompilerHelpers.HasConversionPathTo(t, ty)))
                    .FirstOrDefault(); // biggest one
            }
            while (thenExpr.Type != elseExpr.Type && thenExpr.Type != convertAllTo
                && lastThen != thenExpr.Type && lastElse != elseExpr.Type);

            if (convertAllTo == null)
                throw new ArgumentException("True and False branches of condition do not have a common type to be converted to");

            expr = Expression.Condition(CompilerHelpers.AsBoolean(condition),
                CompilerHelpers.ConvertToType(thenExpr, convertAllTo),
                CompilerHelpers.ConvertToType(elseExpr, convertAllTo));
            return true;
        }
    }
}
