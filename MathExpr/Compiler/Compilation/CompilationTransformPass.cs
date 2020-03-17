using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace MathExpr.Compiler.Compilation
{
    public interface ICompilationTransformPass<in TSettings>
           : ITransformPass<ICompilationTransformContext<TSettings>, MathExpression, Expression>
    {
    }

    public abstract class CompilationTransformPass<TSettings> : ICompilationTransformPass<TSettings>
    {
        public virtual Expression ApplyTo(MathExpression expr, ICompilationTransformContext<TSettings> ctx)
            => expr switch
            {
                Syntax.BinaryExpression b => ApplyTo(b, ctx),
                Syntax.UnaryExpression b => ApplyTo(b, ctx),
                Syntax.MemberExpression b => ApplyTo(b, ctx),
                VariableExpression b => ApplyTo(b, ctx),
                FunctionExpression b => ApplyTo(b, ctx),
                LiteralExpression b => ApplyTo(b, ctx),
                CustomDefinitionExpression b => ApplyTo(b, ctx),
                _ => throw new ArgumentException("Unknown expression type", nameof(expr))
            };

        public abstract Expression ApplyTo(Syntax.BinaryExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(Syntax.UnaryExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(Syntax.MemberExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(VariableExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(FunctionExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(LiteralExpression expr, ICompilationTransformContext<TSettings> ctx);
        public abstract Expression ApplyTo(CustomDefinitionExpression expr, ICompilationTransformContext<TSettings> ctx);
    }
}
