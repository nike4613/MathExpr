using MathExpr.Compiler.OptimizationPasses;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public interface IOptimizationContext<out TSettings>
    {
        TSettings Settings { get; }
    }

    public class OptimizationContext<TSettings> : IOptimizationContext<TSettings>
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
