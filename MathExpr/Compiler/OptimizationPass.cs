using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public interface IOptimizationPass<in TSettings> 
        : ITransformPass<IOptimizationContext<TSettings>, MathExpression, MathExpression>
    {
    }

    public abstract class OptimizationPass : OptimizationPass<object?> { }
    public abstract class OptimizationPass<TSettings> : IOptimizationPass<TSettings>
    { 
        public virtual MathExpression ApplyTo(MathExpression expr, IOptimizationContext<TSettings> ctx)
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

        public virtual MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<TSettings> ctx)
            => new BinaryExpression(expr.Type, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList());
        public virtual MathExpression ApplyTo(UnaryExpression expr, IOptimizationContext<TSettings> ctx)
            => new UnaryExpression(expr.Type, ApplyTo(expr.Argument, ctx));
        public virtual MathExpression ApplyTo(MemberExpression expr, IOptimizationContext<TSettings> ctx)
            => new MemberExpression(ApplyTo(expr.Target, ctx), expr.MemberName);
        public virtual MathExpression ApplyTo(VariableExpression expr, IOptimizationContext<TSettings> ctx) 
            => expr;
        public virtual MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<TSettings> ctx)
            => new FunctionExpression(expr.Name, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList(), expr.IsPrime);
        public virtual MathExpression ApplyTo(LiteralExpression expr, IOptimizationContext<TSettings> ctx) 
            => expr;
        public virtual MathExpression ApplyTo(CustomDefinitionExpression expr, IOptimizationContext<TSettings> ctx)
            => new CustomDefinitionExpression(expr.FunctionName, expr.ArgumentList, ApplyTo(expr.Definition, ctx), ApplyTo(expr.Value, ctx));
    }
}
