﻿using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization
{
    /// <summary>
    /// A specialization of <see cref="ITransformPass{TContext, TFrom, TTo}"/> taking an <see cref="IOptimizationContext{TSettings}"/>
    /// and transforming a <see cref="MathExpression"/> to itself, as an optimization.
    /// </summary>
    /// <typeparam name="TSettings">the settings type that the implementer needs</typeparam>
    public interface IOptimizationPass<in TSettings> 
        : ITransformPass<IOptimizationContext<TSettings>, MathExpression, MathExpression>
    {
    }

    /// <summary>
    /// A basic specialization of <see cref="OptimizationPass{TSettings}"/> that doesn't need any settings.
    /// </summary>
    public abstract class OptimizationPass : OptimizationPass<object?> { }
    /// <summary>
    /// A stub implementation of <see cref="IOptimizationPass{TSettings}"/> that visits and reconstructs the entire expression tree,
    /// allowing implementers to inject themselves into any individual tree node type.
    /// </summary>
    /// <typeparam name="TSettings">the settings type that the implementer needs</typeparam>
    public abstract class OptimizationPass<TSettings> : IOptimizationPass<TSettings>
    {
        // TODO: switch this with an actual pass-based system that isn't nearly infinitely recursive (this wouldn't be as much of an issue if Roslyn emitted tailcalls)
        /// <summary>
        /// The main application method for optimizing an expression. It dispatches to the specific overloads, and depending on their out parameter,
        /// forwards the result to the context's <see cref="ITransformContext{TSettings, TFrom, TTo}.Transform(TFrom)"/>.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <param name="ctx">the context that manages the optimization</param>
        /// <returns>the optimized expression</returns>
        /// <seealso cref="ApplyTo(UnaryExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(BinaryExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(MemberExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(VariableExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(FunctionExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(LiteralExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(StringExpression, IOptimizationContext{TSettings}, out bool)"/>
        /// <seealso cref="ApplyTo(CustomDefinitionExpression, IOptimizationContext{TSettings}, out bool)"/>
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
                StringExpression b => ApplyTo(b, ctx, out transformResult),
                CustomDefinitionExpression b => ApplyTo(b, ctx, out transformResult),
                _ => throw new ArgumentException("Unknown expression type", nameof(expr))
            };
            if (transformResult)
                return ctx.Transform(result);
            else return result;
        }

        private static T SequenceExpressions<T, U>(U _, T r) => r;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public virtual MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new BinaryExpression(expr.Type, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList()))
                .WithToken(expr.Token);
        public virtual MathExpression ApplyTo(UnaryExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new UnaryExpression(expr.Type, ApplyTo(expr.Argument, ctx)))
                .WithToken(expr.Token);
        public virtual MathExpression ApplyTo(MemberExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new MemberExpression(ApplyTo(expr.Target, ctx), expr.MemberName))
                .WithToken(expr.Token);
        public virtual MathExpression ApplyTo(VariableExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult) 
            => SequenceExpressions(transformResult = true, expr);
        public virtual MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, new FunctionExpression(expr.Name, expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList(), expr.IsUserDefined))
                .WithToken(expr.Token);
        public virtual MathExpression ApplyTo(LiteralExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult) 
            => SequenceExpressions(transformResult = true, expr);
        public virtual MathExpression ApplyTo(StringExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, expr);
        public virtual MathExpression ApplyTo(CustomDefinitionExpression expr, IOptimizationContext<TSettings> ctx, out bool transformResult)
            => SequenceExpressions(transformResult = true, 
                new CustomDefinitionExpression(expr.FunctionName, expr.ParameterList, ApplyTo(expr.Definition, ctx), ApplyTo(expr.Value, ctx)))
                .WithToken(expr.Token);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
