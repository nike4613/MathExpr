using MathExpr.Compiler.OptimizationPasses;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public static class OptimizationContext
    {
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, IEnumerable<IOptimizationPass<TSettings>> passes)
            => new OptimizationContext<TSettings>(passes, settings);
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, params IOptimizationPass<TSettings>[] passes)
            => new OptimizationContext<TSettings>(passes, settings);
    }

    public class OptimizationContext<TSettings> : ITransformContext<TSettings>
    {
        private readonly List<IOptimizationPass<TSettings>> passes;

        public IEnumerable<IOptimizationPass<TSettings>> Passes => passes;

        public TSettings Settings { get; set; }

        public OptimizationContext(IEnumerable<IOptimizationPass<TSettings>> passes, TSettings settings)
        {
            this.passes = passes.ToList();
            Settings = settings;
        }

        /// <summary>
        /// Runs all optimization passes, in order, until none of them make any
        /// changes to <see cref="MathExpression.Size"/>.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public MathExpression Optimize(MathExpression expr)
        {
            var lastSize = 0;
            while (lastSize != expr.Size)
            {
                lastSize = expr.Size;
                expr = expr.PipeThrough(Passes, (p, e) => p.ApplyTo(e, this));
            }
            return expr;
        }

        /// <summary>
        /// Runs only passes deriving from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">the type of the passes to run</typeparam>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public MathExpression RunPass<T>(MathExpression expr) where T : IOptimizationPass<TSettings>
            => expr.PipeThrough(Passes.Where(p => p is T), (p, e) => p.ApplyTo(e, this));
    }
}
