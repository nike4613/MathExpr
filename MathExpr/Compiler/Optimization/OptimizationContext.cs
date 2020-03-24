using MathExpr.Compiler.Optimization.Passes;
using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization
{
    /// <summary>
    /// A specialization of <see cref="ITransformContext{TSettings, TFrom, TTo}"/> for transforming a <see cref="MathExpression"/>
    /// to itself as an optimizer.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public interface IOptimizationContext<out TSettings> : ITransformContext<TSettings, MathExpression, MathExpression>
    {
    }

    /// <summary>
    /// A helper class with utilities to more easily create <see cref="OptimizationContext{TSettings}"/>s with type deduction.
    /// </summary>
    public static class OptimizationContext
    {
        /// <summary>
        /// Creates a <see cref="OptimizationContext{TSettings}"/> with the given settings and optimization passes.
        /// </summary>
        /// <typeparam name="TSettings">the type of settings for the new context</typeparam>
        /// <param name="settings">the settings to initialize the context with</param>
        /// <param name="passes">the set of passes to initialize the context with</param>
        /// <returns>the new context</returns>
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, params IOptimizationPass<TSettings>[] passes)
            => CreateWith(settings, passes.AsEnumerable());
        /// <summary>
        /// Creates a <see cref="OptimizationContext{TSettings}"/> with the given settings and optimization passes.
        /// </summary>
        /// <typeparam name="TSettings">the type of settings for the new context</typeparam>
        /// <param name="settings">the settings to initialize the context with</param>
        /// <param name="passes">the set of passes to initialize the context with</param>
        /// <returns>the new context</returns>
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, IEnumerable<IOptimizationPass<TSettings>> passes)
            => new OptimizationContext<TSettings>(passes, settings);

        /// <summary>
        /// Creates a <see cref="OptimizationContext{TSettings}"/> with the given settings and optimization passes, alongside default passes.
        /// </summary>
        /// <param name="createdOverride">the settings to initialize the context with, or <see langword="null"/> to use a new settings object</param>
        /// <param name="morePasses">the set of passes to initialize the context with</param>
        /// <returns>the new context</returns>
        public static OptimizationContext<DefaultOptimizationSettings> CreateDefault(DefaultOptimizationSettings? createdOverride = null,
            params IOptimizationPass<DefaultOptimizationSettings>[] morePasses)
            => CreateDefault(createdOverride, morePasses.AsEnumerable());
        /// <summary>
        /// Creates a <see cref="OptimizationContext{TSettings}"/> with the given settings and optimization passes, alongside default passes.
        /// </summary>
        /// <param name="createdOverride">the settings to initialize the context with, or <see langword="null"/> to use a new settings object</param>
        /// <param name="morePasses">the set of passes to initialize the context with</param>
        /// <returns>the new context</returns>
        public static OptimizationContext<DefaultOptimizationSettings> CreateDefault(DefaultOptimizationSettings? createdOverride,
            IEnumerable<IOptimizationPass<DefaultOptimizationSettings>> morePasses)
        {
            createdOverride ??= new DefaultOptimizationSettings();
            return CreateWith(createdOverride, morePasses
                .Append(new UserFunctionInlinePass())
                .Append(new BuiltinExponentSimplificationPass())
                .Append(new BinaryExpressionCombinerPass())
                .Append(new BuiltinExponentConstantReductionPass())
                .Append(new LiteralCombinerPass()));
        }
    }

    /// <summary>
    /// An optimization context for easily running multiple optimization passes on expressions.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public class OptimizationContext<TSettings> : DataProvidingContext, IOptimizationContext<TSettings>
    {
        private readonly List<IOptimizationPass<TSettings>> passes;

        /// <summary>
        /// The passes registered to be used.
        /// </summary>
        public IEnumerable<IOptimizationPass<TSettings>> Passes => passes;

        /// <summary>
        /// The settings being provided.
        /// </summary>
        public TSettings Settings { get; set; }

        /// <summary>
        /// Creates a new context with the given passes and settings.
        /// </summary>
        /// <param name="passes">the passes to initialize whit</param>
        /// <param name="settings">the settings to initialize with</param>
        public OptimizationContext(IEnumerable<IOptimizationPass<TSettings>> passes, TSettings settings)
        {
            this.passes = passes.ToList();
            Settings = settings;
        }

        /// <summary>
        /// Sets this context's parent data context.
        /// </summary>
        /// <param name="newParent">the context to parent this to</param>
        public void SetParentDataContext(DataProvidingContext? newParent) => SetParent(newParent);

        private class SubContext : DataProvidingContext, IOptimizationContext<TSettings>
        {
            private readonly OptimizationContext<TSettings> owner;
            private readonly int currentIndex;

            public SubContext(OptimizationContext<TSettings> own, int index = 0) : this(own, own, index)
            {
            }
            private SubContext(SubContext parent) : this(parent, parent.owner, parent.currentIndex + 1)
            {
            }
            private SubContext(DataProvidingContext? parent, OptimizationContext<TSettings> own, int index = 0) : base(parent)
            {
                owner = own;
                currentIndex = index;
            }

            public TSettings Settings => owner.Settings;

            private SubContext? _child = null;
            private SubContext Child
            {
                get
                {
                    if (_child == null)
                        _child = new SubContext(this);
                    else
                        _child.DataStore.Clear();
                    return _child;
                }
            }

            public MathExpression Transform(MathExpression from)
            {
                if (currentIndex >= owner.passes.Count)
                    return from;
                else
                    return owner.passes[currentIndex].ApplyTo(from, Child);
            }
        }

        /// <summary>
        /// Runs the expression through all optimization passes.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public MathExpression Optimize(MathExpression expr)
            => new SubContext(this).Transform(expr);

        MathExpression ITransformContext<TSettings, MathExpression, MathExpression>.Transform(MathExpression from)
            => Optimize(from);
    }
}
