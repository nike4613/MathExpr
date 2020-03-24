using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Optimization;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;


namespace MathExpr.Compiler
{
    /// <summary>
    /// Static utilities regarding <see cref="ExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/>s.
    /// </summary>
    public static class ExpressionCompiler
    {
        /// <summary>
        /// An instance of a <see cref="DefaultExpressionCompiler"/>, provided for more readable callsites.
        /// </summary>
        public static DefaultExpressionCompiler Default => new DefaultExpressionCompiler();
    }

    /// <summary>
    /// An easy-to-use wrapper around both <see cref="OptimizationContext{TSettings}"/> and <see cref="CompilationTransformContext{TSettings}"/>
    /// for easily optimizing and compiling <see cref="MathExpression"/>s to <see cref="Expression"/>s.
    /// </summary>
    /// <typeparam name="TOptimizerSettings">the type of optimizer settings to use</typeparam>
    /// <typeparam name="TCompilerSettings">the type of compiler settings to use</typeparam>
    public class ExpressionCompiler<TOptimizerSettings, TCompilerSettings> : IDataContext
    {
        /// <summary>
        /// The optimizer settings to use.
        /// </summary>
        public TOptimizerSettings OptimizerSettings { get; }
        /// <summary>
        /// The compiler settings to use.
        /// </summary>
        public TCompilerSettings CompilerSettings { get; }

        /// <summary>
        /// The list of optimizer passes to use during optimization.
        /// </summary>
        public List<IOptimizationPass<TOptimizerSettings>> OptimizerPasses { get; } = new List<IOptimizationPass<TOptimizerSettings>>();
        /// <summary>
        /// The compiler backend to use when compiling.
        /// </summary>
        public ICompilationTransformPass<TCompilerSettings> Compiler { get; set; }

        /// <summary>
        /// Constructs a compiler wrapper with the given optimizer and compiler settings, along with the specified compiler backend.
        /// </summary>
        /// <param name="optimizerSettings">the optimizer settings to initialize with</param>
        /// <param name="compilerSettings">the compiler settings to initialize with</param>
        /// <param name="compiler">the compiler backend to initialize with</param>
        public ExpressionCompiler(TOptimizerSettings optimizerSettings, TCompilerSettings compilerSettings, ICompilationTransformPass<TCompilerSettings> compiler)
        {
            OptimizerSettings = optimizerSettings;
            CompilerSettings = compilerSettings;
            Compiler = compiler;
        }

        /// <summary>
        /// The data store implementation underlying this <see cref="ExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/>.
        /// </summary>
        protected DataProvidingContext SharedDataStore { get; } = new DataContextImpl();

        private sealed class DataContextImpl : DataProvidingContext { }

        /// <summary>
        /// Optimizes a <see cref="MathExpression"/> using the passes in <see cref="OptimizerPasses"/> and the settings in
        /// <see cref="OptimizerSettings"/>.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public virtual MathExpression Optimize(MathExpression expr)
        {
            var ctx = OptimizationContext.CreateWith(OptimizerSettings, OptimizerPasses);
            ctx.SetParentDataContext(SharedDataStore);
            return ctx.Optimize(expr);
        }

        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to an <see cref="Expression"/> using the settings in <see cref="CompilerSettings"/>
        /// and the compiler backend in <see cref="Compiler"/>, optionally optimizing first.
        /// </summary>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize first</param>
        /// <returns>the compiled <see cref="Expression"/></returns>
        /// <seealso cref="Optimize(MathExpression)"/>
        public virtual Expression CompileToExpression(MathExpression expr, bool optimize = true)
        {
            if (optimize) expr = Optimize(expr);
            var ctx = CompilationTransformContext.CreateWith(CompilerSettings, Compiler);
            ctx.SetParentDataContext(SharedDataStore);
            return ctx.Transform(expr);
        }

        /// <inheritdoc/>
        public TData GetOrCreateData<TScope, TData>(Func<TData> creator)
            => SharedDataStore.GetOrCreateData<TScope, TData>(creator);
        /// <inheritdoc/>
        public void SetData<TScope, TData>(TData data)
            => SharedDataStore.SetData<TScope, TData>(data);
    }
}
