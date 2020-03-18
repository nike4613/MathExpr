using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Optimization;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;


namespace MathExpr.Compiler
{
    public static class ExpressionCompiler
    {
        public static DefaultExpressionCompiler Default { get; } = new DefaultExpressionCompiler();
    }

    public class ExpressionCompiler<TOptimizerSettings, TCompilerSettings>
    {
        public TOptimizerSettings OptimizerSettings { get; }
        public TCompilerSettings CompilerSettings { get; }

        public List<IOptimizationPass<TOptimizerSettings>> OptimizerPasses { get; } = new List<IOptimizationPass<TOptimizerSettings>>();
        public ICompilationTransformPass<TCompilerSettings> Compiler { get; set; }

        public ExpressionCompiler(TOptimizerSettings optimizer, TCompilerSettings compilerSettings, ICompilationTransformPass<TCompilerSettings> compiler)
        {
            OptimizerSettings = optimizer;
            CompilerSettings = compilerSettings;
            Compiler = compiler;
        }

        public MathExpression Optimize(MathExpression expr)
        {
            var ctx = OptimizationContext.CreateWith(OptimizerSettings, OptimizerPasses);
            return ctx.Optimize(expr);
        }

        public Expression CompileToExpression(MathExpression expr, bool optimize = true)
        {
            if (optimize) expr = Optimize(expr);
            var ctx = CompilationTransformContext.CreateWith(CompilerSettings, Compiler);
            return ctx.Transform(expr);
        }
    }
}
