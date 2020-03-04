using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public interface IOptimizationPass : ITransformPass<OptimizationContext, MathExpression, MathExpression>
    {
    }

    public abstract class OptimizationPass : IOptimizationPass
    { 
        public virtual MathExpression ApplyTo(MathExpression expr, OptimizationContext ctx)
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

        public virtual MathExpression ApplyTo(BinaryExpression expr, OptimizationContext ctx)
            => new BinaryExpression(expr.Type, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList());
        public virtual MathExpression ApplyTo(UnaryExpression expr, OptimizationContext ctx)
            => new UnaryExpression(expr.Type, ApplyTo(expr.Argument, ctx));
        public virtual MathExpression ApplyTo(MemberExpression expr, OptimizationContext ctx)
            => new MemberExpression(ApplyTo(expr.Target, ctx), expr.MemberName);
        public virtual MathExpression ApplyTo(VariableExpression expr, OptimizationContext ctx) => expr;
        public virtual MathExpression ApplyTo(FunctionExpression expr, OptimizationContext ctx)
            => new FunctionExpression(expr.Name, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList(), expr.IsPrime);
        public virtual MathExpression ApplyTo(LiteralExpression expr, OptimizationContext ctx) => expr;
        public virtual MathExpression ApplyTo(CustomDefinitionExpression expr, OptimizationContext ctx)
            => new CustomDefinitionExpression(expr.FunctionName, expr.ArgumentList, ApplyTo(expr.Definition, ctx), ApplyTo(expr.Value, ctx));
    }
}
