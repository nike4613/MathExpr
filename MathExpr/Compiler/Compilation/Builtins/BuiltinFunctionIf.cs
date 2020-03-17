using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    public class BuiltinFunctionIf : IBuiltinFunction<object?>
    {
        public const string ConstName = "if";
        public string Name => ConstName;

        public int ParamCount => 3; // condition, if true, if false

        public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationTransformContext<object?> ctx, ITypeHintHandler hintHandler, out Expression expr)
        {
            var condition = arguments.First();
            var thenExpr = arguments[1];
            var elseExpr = arguments[2];

            var compiledCondition = hintHandler.TransformWithHint(condition, typeof(bool), ctx);
            expr = Expression.Condition(compiledCondition, ctx.Transform(thenExpr), ctx.Transform(elseExpr));
            return true;
        }
    }
}
