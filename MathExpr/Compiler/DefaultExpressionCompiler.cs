using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Compilation.Passes;
using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Compiler.Optimization;
using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler
{
    /// <summary>
    /// A specialization of <see cref="ExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> using the default settings types
    /// <see cref="DefaultOptimizationSettings"/> and <see cref="DefaultLinqExpressionCompilerSettings"/>.
    /// </summary>
    public class DefaultExpressionCompiler : LinqExpressionCompiler<DefaultOptimizationSettings, DefaultLinqExpressionCompilerSettings>
    {
        /// <summary>
        /// Constructs an expression compiler with default constructed optimization and compilation settings.
        /// </summary>
        public DefaultExpressionCompiler() : this(new DefaultOptimizationSettings(), new DefaultLinqExpressionCompilerSettings())
        {
        }

        /// <summary>
        /// Constructs an expression compiler with the specified optimization and compilation settings.
        /// </summary>
        /// <param name="optimizerSettings">the optmization settings to initialize with</param>
        /// <param name="compilerSettings">the compielr settings to initialize with</param>
        public DefaultExpressionCompiler(DefaultOptimizationSettings optimizerSettings, DefaultLinqExpressionCompilerSettings compilerSettings) 
            : base(optimizerSettings, compilerSettings, new DefaultLinqExpressionCompiler<DefaultLinqExpressionCompilerSettings>())
        {
        }

        /// <summary>
        /// Optimizes the provided expression using an <see cref="OptimizationContext{TSettings}"/>
        /// created with <see cref="OptimizationContext.CreateDefault(DefaultOptimizationSettings?, IEnumerable{IOptimizationPass{DefaultOptimizationSettings}})"/>
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public override MathExpression Optimize(MathExpression expr)
        {
            var ctx = OptimizationContext.CreateDefault(OptimizerSettings, OptimizerPasses);
            ctx.SetParentDataContext(SharedDataStore);
            return ctx.Optimize(expr);
        }
    }
}
