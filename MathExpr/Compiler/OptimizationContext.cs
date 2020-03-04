using MathExpr.Compiler.OptimizationPasses;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public class OptimizationContext
    {
        private readonly List<IOptimizationPass> passes;

        public IEnumerable<IOptimizationPass> Passes => passes;

        // TODO: better interface here
        public static List<IOptimizationPass> DefaultPasses = new List<IOptimizationPass>
        {
            new BinaryExpressionCombinerPass(),
            new LiteralCombinerPass()
        };

        public OptimizationContext() : this(DefaultPasses)
        {
        }

        public OptimizationContext(IEnumerable<IOptimizationPass> passes)
        {
            this.passes = passes.ToList();
        }

        public MathExpression RunOver(MathExpression expr)
            => expr.PipeThrough(Passes, (p, e) => p.ApplyTo(e, this));
    }
}
