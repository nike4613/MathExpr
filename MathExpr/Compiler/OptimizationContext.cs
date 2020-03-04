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

        public MathExpression Optimize(MathExpression expr)
            => expr.PipeThrough(Passes, (p, e) => p.ApplyTo(e, this));
    }
}
