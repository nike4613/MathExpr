﻿using MathExpr.Compiler.Compilation;
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
    public class DefaultExpressionCompiler : ExpressionCompiler<DefaultOptimizationSettings, DefaultLinqExpressionCompilerSettings>
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
        /// created with <see cref="OptimizationContext.CreateDefault(DefaultOptimizationSettings?, IEnumerable{IOptimizationPass{DefaultOptimizationSettings}})"/>.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public override MathExpression Optimize(MathExpression expr)
        {
            var ctx = OptimizationContext.CreateDefault(OptimizerSettings, OptimizerPasses);
            ctx.SetParentDataContext(SharedDataStore);
            return ctx.Optimize(expr);
        }

        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to an <see cref="Expression"/>, optionally optimizing it first.
        /// </summary>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize <paramref name="expr"/></param>
        /// <returns>the compiled <see cref="Expression"/></returns>
        /// <seealso cref="Optimize(MathExpression)"/>
        public override Expression CompileToExpression(MathExpression expr, bool optimize = true)
        {
            if (optimize)
            {
                expr = Optimize(expr);
            }
            return base.CompileToExpression(expr, false);
        }

        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optionally optimizing first, using the specified
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize the expression before compiling</param>
        /// <param name="compile">the delegate to use to compile the resulting <see cref="Expression{TDelegate}"/> into a delegate</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
        {
            var parameters = DelegateInformation<TDelegate>.ParamTypes
                .Zip(parameterNames, (type, name) => (type, name))
                .Select(t => (t.name, param: Expression.Parameter(t.type, t.name)))
                .ToList();

            if (parameters.Count != DelegateInformation<TDelegate>.ParamTypes.Length)
                throw new ArgumentException("Incorrect number of argument names", nameof(parameterNames));

            CompilerSettings.ExpectReturn = DelegateInformation<TDelegate>.ReturnType;
            CompilerSettings.ParameterMap.Clear();
            foreach (var (name, param) in parameters)
                CompilerSettings.ParameterMap.Add(new VariableExpression(name), param);

            DomainRestrictionSettings.GetDomainRestrictionsFor(this).Clear(); // ensure that restrictions are defined locally

            return compile(
                Expression.Lambda<TDelegate>(
                    CompileToExpression(expr, optimize),
                    parameters.Select(p => p.param)));
        }
        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optionally optimizing first, using the specified
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize the expression before compiling</param>
        /// <param name="compile">the delegate to use to compile the resulting <see cref="Expression{TDelegate}"/> into a delegate</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile(expr, optimize, compile, parameterNames.AsEnumerable());
        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optionally optimizing first, using the default 
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize the expression before compiling</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, e => e.Compile(), parameterNames);
        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optionally optimizing first, using the default 
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize the expression before compiling</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, parameterNames.AsEnumerable());
        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optimizing first, using the default 
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, true, e => e.Compile(), parameterNames);
        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of a specified type, optimizing first, using the default 
        /// <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.
        /// </remarks>
        /// <typeparam name="TDelegate">the type of delegate to compile to</typeparam>
        /// <param name="expr">the expression to compile</param>
        /// <param name="parameterNames">an ordered list of parameter names corresponding to delegate arguments</param>
        /// <returns>the compiled delegate</returns>
        public TDelegate Compile<TDelegate>(MathExpression expr, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, parameterNames.AsEnumerable());
    }
}
