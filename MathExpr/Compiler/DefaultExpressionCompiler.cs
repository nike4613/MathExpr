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
    }
}
