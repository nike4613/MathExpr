using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MathExpr.Compiler.Compilation.Settings
{
    /// <summary>
    /// Compiles a factorial operation given an argument.
    /// </summary>
    /// <param name="argument">the argument to apply factorial to</param>
    /// <returns>the resulting value</returns>
    public delegate Expression TypedFactorialCompiler(Expression argument);
    /// <summary>
    /// Settings to control compilation to a Linq expression.
    /// </summary>
    public interface ICompileToLinqExpressionSettings<TSettings> : 
        IDomainRestrictionSettings,
        IBuiltinFunctionCompilerSettings<TSettings>
    {
        // TODO: pull the inherited interfaces out
        /// <summary>
        /// The type that the resulting expression is expected to return.
        /// </summary>
        Type ExpectReturn { get; }
        /// <summary>
        /// A map of variables to <see cref="ParameterExpression"/>s.
        /// </summary>
        IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; }

        /// <summary>
        /// A dictionary of parameter types to <see cref="TypedFactorialCompiler"/>s to use to compile
        /// factorial operations.
        /// </summary>
        IDictionary<Type, TypedFactorialCompiler> TypedFactorialCompilers { get; }
        /// <summary>
        /// A list of <see cref="ISpecialBinaryOperationCompiler"/> to use to try to compile a power
        /// operation.
        /// </summary>
        IList<ISpecialBinaryOperationCompiler> PowerCompilers { get; }
    }

    /// <summary>
    /// An interface representing the writable portions of <see cref="ICompileToLinqExpressionSettings{TSettings}"/> for
    /// those that need it.
    /// </summary>
    public interface IWritableCompileToLinqExpressionSettings
    {
        /// <summary>
        /// Sets the type that the resulting expression is expected to return.
        /// </summary>
        Type ExpectReturn { get; set; }
    }
}
