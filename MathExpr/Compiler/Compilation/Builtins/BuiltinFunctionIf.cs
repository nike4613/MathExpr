using MathExpr.Syntax;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// The name of this builtin function.
        /// </summary>
        public string Name => ConstName;
        /// <summary>
        /// The number of parameters that this builtin supports
        /// </summary>
        public int ParamCount => 3; // condition, if true, if false

        /// <summary>
        /// Attempts to compile a call to the builtin with the given arguments in a context.
        /// </summary>
        /// <param name="arguments">the arguments provided in the call</param>
        /// <param name="ctx">the compilation context</param>
        /// <param name="hintHandler">the handler for compiling an expression with a preferred type</param>
        /// <param name="expr">the <see cref="Expression"/> for the compiled call</param>
        /// <returns><see langword="true"/> if the compilation was successful, <see langword="false"/> otherwise</returns>
        public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationTransformContext<object?> ctx, ITypeHintHandler hintHandler, out Expression expr)
        {
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
