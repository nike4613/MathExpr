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
    /// A static class containing helpers to more easily create <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/>s.
    /// </summary>
    public static class LinqExpressionCompiler
    {
        /// <summary>
        /// Creates a new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> using the specified settings and compiler.
        /// </summary>
        /// <typeparam name="TOptimizerSettings">The type of the optimizer settings.</typeparam>
        /// <typeparam name="TCompilerSettings">The type of the compiler settings.</typeparam>
        /// <param name="optimizerSettings">The optimizer settings to use.</param>
        /// <param name="compilerSettings">The compiler settings to use.</param>
        /// <param name="compiler">The compiler to use.</param>
        /// <returns>The new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/>.</returns>
        /// <seealso cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}(TOptimizerSettings, TCompilerSettings, ICompiler{TCompilerSettings})"/>
        public static LinqExpressionCompiler<TOptimizerSettings, TCompilerSettings> Create<TOptimizerSettings, TCompilerSettings>(
            TOptimizerSettings optimizerSettings, 
            TCompilerSettings compilerSettings, 
            ICompiler<TCompilerSettings> compiler
        ) where TCompilerSettings : ICompileToLinqExpressionSettings<TCompilerSettings>, IBuiltinFunctionCompilerSettings<TCompilerSettings>, IWritableCompileToLinqExpressionSettings
            => new LinqExpressionCompiler<TOptimizerSettings, TCompilerSettings>(optimizerSettings, compilerSettings, compiler);
        /// <summary>
        /// Creates a new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> using the specified settings, using the default compiler.
        /// </summary>
        /// <typeparam name="TOptimizerSettings">The type of the optimizer settings.</typeparam>
        /// <typeparam name="TCompilerSettings">The type of the compiler settings.</typeparam>
        /// <param name="optimizerSettings">The optimizer settings to use.</param>
        /// <param name="compilerSettings">The compiler settings to use.</param>
        /// <returns>The new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/>.</returns>
        /// <seealso cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}(TOptimizerSettings, TCompilerSettings)"/>
        public static LinqExpressionCompiler<TOptimizerSettings, TCompilerSettings> Create<TOptimizerSettings, TCompilerSettings>(
            TOptimizerSettings optimizerSettings,
            TCompilerSettings compilerSettings
        ) where TCompilerSettings : ICompileToLinqExpressionSettings<TCompilerSettings>, IBuiltinFunctionCompilerSettings<TCompilerSettings>, IWritableCompileToLinqExpressionSettings
            => new LinqExpressionCompiler<TOptimizerSettings, TCompilerSettings>(optimizerSettings, compilerSettings);
    }

    /// <summary>
    /// A specialization of <see cref="ExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> that provides a fairly nice interface for use.
    /// </summary>
    /// <remarks>
    /// Each instance of this compiler may be used by only one thread at a time.
    /// </remarks>
    /// <typeparam name="TOptimizerSettings">The type of the optimizer settings.</typeparam>
    /// <typeparam name="TCompilerSettings">The type of the compiler settings.</typeparam>
    public class LinqExpressionCompiler<TOptimizerSettings, TCompilerSettings>
        : ExpressionCompiler<TOptimizerSettings, TCompilerSettings>
        where TCompilerSettings : 
            ICompileToLinqExpressionSettings<TCompilerSettings>,
            IBuiltinFunctionCompilerSettings<TCompilerSettings>,
            IWritableCompileToLinqExpressionSettings
    {
        /// <summary>
        /// Creates a new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> using the specified settings and compiler.
        /// </summary>
        /// <param name="optimizerSettings">The optimizer settings to use.</param>
        /// <param name="compilerSettings">The compiler settings to use.</param>
        /// <param name="compiler">The compiler to use.</param>
        public LinqExpressionCompiler(TOptimizerSettings optimizerSettings, TCompilerSettings compilerSettings, ICompiler<TCompilerSettings> compiler) 
            : base(optimizerSettings, compilerSettings, compiler)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LinqExpressionCompiler{TOptimizerSettings, TCompilerSettings}"/> using the specified settings, with the default compiler.
        /// </summary>
        /// <remarks>
        /// The compiler that this constructor uses is <see cref="DefaultLinqExpressionCompiler{TSettings}"/>.
        /// </remarks>
        /// <param name="optimizerSettings">The optimizer settings to use.</param>
        /// <param name="compilerSettings">The compiler settings to use.</param>
        public LinqExpressionCompiler(TOptimizerSettings optimizerSettings, TCompilerSettings compilerSettings)
            : this(optimizerSettings, compilerSettings, new DefaultLinqExpressionCompiler<TCompilerSettings>())
        {
        }

        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to an <see cref="Expression"/>, optionally optimizing it first.
        /// </summary>
        /// <param name="expr">the expression to compile</param>
        /// <param name="optimize">whether or not to optimize <paramref name="expr"/></param>
        /// <returns>the compiled <see cref="Expression"/></returns>
        /// <seealso cref="ExpressionCompiler{TOptimizerSettings, TCompilerSettings}.Optimize(MathExpression)"/>
        public override Expression CompileToExpression(MathExpression expr, bool optimize = true)
        {
            if (optimize)
            {
                expr = Optimize(expr);
            }
            return base.CompileToExpression(expr, false);
        }

        /// <summary>
        /// Compiles a <see cref="MathExpression"/> to a delegate of the specified type, optionally optimizing first, using the specified
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
        {
            var parameters = DelegateInformation<TDelegate>.ParamTypes
                .Zip(parameterNames, Helpers.Tuple)
                .Select(t => (name: t.Second, param: Expression.Parameter(t.First, t.Second)))
                .ToList();

            if (parameters.Count != DelegateInformation<TDelegate>.ParamTypes.Length)
                throw new ArgumentException("Incorrect number of argument names", nameof(parameterNames));

            (CompilerSettings as IWritableCompileToLinqExpressionSettings).ExpectReturn = DelegateInformation<TDelegate>.ReturnType;
            CompilerSettings.ParameterMap.Clear();
            foreach (var (name, param) in parameters)
                CompilerSettings.ParameterMap.Add(new VariableExpression(name), param);

            if (CompilerSettings is IDomainRestrictionSettings)
                DomainRestrictionSettings.GetDomainRestrictionsFor(this).Clear(); // ensure that restrictions are defined locally

            return compile(
                Expression.Lambda<TDelegate>(
                    CompileToExpression(expr, optimize),
                    parameters.Select(p => p.param)));
        }
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optionally optimizing
        /// first, using the specified <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="optimize">Whether or not to optimize the expression before compiling.</param>
        /// <param name="compile">The delegate to use to compile the <see cref="Expression{TDelegate}"/> into a delegate.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile(MathExpression.Parse(expr), optimize, compile, parameterNames);
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile(expr, optimize, compile, parameterNames.AsEnumerable());
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optionally optimizing
        /// first, using the specified <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="optimize">Whether or not to optimize the expression before compiling.</param>
        /// <param name="compile">The delegate to use to compile the <see cref="Expression{TDelegate}"/> into a delegate.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, params string[] parameterNames)
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, e => e.Compile(), parameterNames);
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optionally optimizing
        /// first, using the default <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="optimize">Whether or not to optimize the expression before compiling.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, bool optimize, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(MathExpression.Parse(expr), optimize, parameterNames);
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, parameterNames.AsEnumerable());
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optionally optimizing
        /// first, using the default <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="optimize">Whether or not to optimize the expression before compiling.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, bool optimize, params string[] parameterNames)
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, true, e => e.Compile(), parameterNames);
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optimizing
        /// first, using the default <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, IEnumerable<string> parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(MathExpression.Parse(expr), parameterNames);
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
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        public TDelegate Compile<TDelegate>(MathExpression expr, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, parameterNames.AsEnumerable());
        /// <summary>
        /// Compiles a <see cref="string"/> representing a <see cref="MathExpression"/> to a delegate of the specified type, optimizing
        /// first, using the default <see cref="Expression{TDelegate}"/> compiler, with the given parameter variable order.
        /// </summary>
        /// <remarks>
        /// <para>The <paramref name="expr"/> given is first passed to <see cref="MathExpression.Parse(string, bool)"/> to parse it into an expression tree.</para>
        /// <para>There must be at least as many elements of <paramref name="parameterNames"/> as there are parameters to <typeparamref name="TDelegate"/>,
        /// even if some of the variables remain unused.</para>
        /// </remarks>
        /// <typeparam name="TDelegate">The type of delegate to compile.</typeparam>
        /// <param name="expr">The expression to compile, as a string.</param>
        /// <param name="parameterNames">An ordered list of parameter names corresponding to delegate arguments.</param>
        /// <returns>The compiled delegate.</returns>
        /// <exception cref="CompilationException">Thrown if there was an error compiling the expression.</exception>
        /// <exception cref="SyntaxException">Thrown if there was an error in the syntax of the expression.</exception>
        public TDelegate Compile<TDelegate>(string expr, params string[] parameterNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, parameterNames.AsEnumerable());
    }
}
