using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    public interface ICompilationTransformPass<in TSettings, TTo>
           : ITransformPass<ICompilationTransformContext<TSettings, TTo>, MathExpression, TTo>
    {
    }

    public abstract class CompilationTransformPass<TSettings, TTo> : ICompilationTransformPass<TSettings, TTo>
    {
        public virtual TTo ApplyTo(MathExpression expr, ICompilationTransformContext<TSettings, TTo> ctx)
            => expr switch
            {
                BinaryExpression b => ApplyTo(b, ctx),
                UnaryExpression b => ApplyTo(b, ctx),
                MemberExpression b => ApplyTo(b, ctx),
                VariableExpression b => ApplyTo(b, ctx),
                FunctionExpression b => ApplyTo(b, ctx),
                LiteralExpression b => ApplyTo(b, ctx),
                CustomDefinitionExpression b => ApplyTo(b, ctx),
                _ => throw new ArgumentException("Unknown expression type", nameof(expr))
            };

        public abstract TTo ApplyTo(BinaryExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(UnaryExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(MemberExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(VariableExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(FunctionExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(LiteralExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
        public abstract TTo ApplyTo(CustomDefinitionExpression expr, ICompilationTransformContext<TSettings, TTo> ctx);
    }
}
