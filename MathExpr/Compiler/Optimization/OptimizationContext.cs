using MathExpr.Compiler.Optimization.Passes;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization
{
    public static class OptimizationContext
    {
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, IEnumerable<IOptimizationPass<TSettings>> passes)
            => new OptimizationContext<TSettings>(passes, settings);
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, params IOptimizationPass<TSettings>[] passes)
            => new OptimizationContext<TSettings>(passes, settings);
    }

    public interface IOptimizationContext<out TSettings> : ITransformContext<TSettings, MathExpression, MathExpression>
    {
    }

    public class OptimizationContext<TSettings>
    {
        private readonly List<IOptimizationPass<TSettings>> passes;

        public IEnumerable<IOptimizationPass<TSettings>> Passes => passes;

        public TSettings Settings { get; set; }

        public OptimizationContext(IEnumerable<IOptimizationPass<TSettings>> passes, TSettings settings)
        {
            this.passes = passes.ToList();
            Settings = settings;
        }

        private class SubContext : DataProvidingTransformContext, IOptimizationContext<TSettings>
        {
            private readonly OptimizationContext<TSettings> owner;
            private readonly int currentIndex;

            public SubContext(OptimizationContext<TSettings> own, int index = 0) : this(null, own, index)
            {
            }
            private SubContext(SubContext parent) : this(parent, parent.owner, parent.currentIndex + 1)
            {
            }
            private SubContext(SubContext? parent, OptimizationContext<TSettings> own, int index = 0) : base(parent)
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
    }
}
