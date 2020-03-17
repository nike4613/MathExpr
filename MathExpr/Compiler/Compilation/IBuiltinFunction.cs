using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    public delegate void SetTypeHintDelegate(ITransformContext ctx, Type? type);
    public interface IBuiltinFunction<in TSettings>
    {
        string Name { get; }
        int ParamCount { get; }
        bool TryCompile(IReadOnlyList<MathExpression> arguments, 
            ICompilationTransformContext<TSettings> context, 
            SetTypeHintDelegate setTypeHint, out Expression expr);
    }

    public abstract class SimpleBuiltinFunction<TSettings> : IBuiltinFunction<TSettings>
    { 
        public abstract string Name { get; }
        public abstract int ParamCount { get; }

        bool IBuiltinFunction<TSettings>.TryCompile(IReadOnlyList<MathExpression> arguments,
            ICompilationTransformContext<TSettings> context,
            SetTypeHintDelegate setTypeHint, out Expression expr)
            => TryCompile(arguments, context, out expr);

        public abstract bool TryCompile(IReadOnlyList<MathExpression> arguments,
            ICompilationTransformContext<TSettings> context,
            out Expression expr);
    }
}
