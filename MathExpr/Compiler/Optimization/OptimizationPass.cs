using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization
{
    public interface IOptimizationPass<in TSettings> 
        : ITransformPass<IOptimizationContext<TSettings>, MathExpression, MathExpression>
    {
    }

    public abstract class OptimizationPass : OptimizationPass<object?> { }
    public abstract class OptimizationPass<TSettings> : IOptimizationPass<TSettings>
    {
        // TODO: there has to be some way to reduce the number of traversals of the tree
        public virtual MathExpression ApplyTo(MathExpression expr, IOptimizationContext<TSettings> ctx)
        {
            bool transformResult;
            var result = expr switch
            {
                BinaryExpression b => ApplyTo(b, ctx, out transformResult),
                UnaryExpression b => ApplyTo(b, ctx, out transformResult),
                MemberExpression b => ApplyTo(b, ctx, out transformResult),
                VariableExpression b => ApplyTo(b, ctx, out transformResult),
                FunctionExpression b => ApplyTo(b, ctx, out transformResult),
                LiteralExpression b => ApplyTo(b, ctx, out transformResult),
                CustomDefinitionExpression b => ApplyTo(b, ctx, out transformResult),
                _ => throw new ArgumentException("Unknown expression type", nameof(expr))
            };
            if (transformResult)
                return ctx.Transform(result);
            else return result;
        }

        public static T SequenceExpressions<T, U>(U _, T r) => r;

        public virtual MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new BinaryExpression(expr.Type, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList()));
        public virtual MathExpression ApplyTo(UnaryExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new UnaryExpression(expr.Type, ApplyTo(expr.Argument, ctx)));
        public virtual MathExpression ApplyTo(MemberExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new MemberExpression(ApplyTo(expr.Target, ctx), expr.MemberName));
        public virtual MathExpression ApplyTo(VariableExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult) 
            => SequenceExpressions(transformResult = true, expr);
        public virtual MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new FunctionExpression(expr.Name, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList(), expr.IsPrime));
        public virtual MathExpression ApplyTo(LiteralExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult) 
            => SequenceExpressions(transformResult = true, expr);
        public virtual MathExpression ApplyTo(CustomDefinitionExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, 
                new CustomDefinitionExpression(expr.FunctionName, expr.ArgumentList, ApplyTo(expr.Definition, ctx), ApplyTo(expr.Value, ctx)));
    }
}
