using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    /// <summary>
    /// An interface providing type hinting facilities during compilation.
    /// </summary>
    /// <remarks>
    /// When an expression is compiled with a type hint, it is not required, nor
    /// guaranteed to be of that type. This is to allow the propagation of strongly
    /// typed values like variables up the expression tree.
    /// </remarks>
    public interface ITypeHintHandler
    {
        /// <summary>
        /// Gets the currently hinted type, if any.
        /// </summary>
        /// <typeparam name="TSettings">the settings type for the context</typeparam>
        /// <param name="ctx">the context being compiled in</param>
        /// <returns>the currently hinted type, or <see langword="null"/> if there is none</returns>
        Type? CurrentHint<TSettings>(ICompilationContext<TSettings> ctx);
        /// <summary>
        /// Transforms <paramref name="expr"/> using the provided type hint in the given context.
        /// </summary>
        /// <typeparam name="TSettings">the settings type for the context</typeparam>
        /// <param name="expr">the expression to transform</param>
        /// <param name="hint">the type hint to provide</param>
        /// <param name="ctx">the context to compile in</param>
        /// <returns>the compiled expression</returns>
        Expression TransformWithHint<TSettings>(MathExpression expr, Type? hint, ICompilationContext<TSettings> ctx);
    }

    /// <summary>
    /// A builtin function that provides a compiler to compile the invocation.
    /// </summary>
    /// <typeparam name="TSettings">the settings type that the implementation requires</typeparam>
    public interface IBuiltinFunction<in TSettings>
    {
        /// <summary>
        /// The name of the function. This is the name that users will use to invoke the function.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Attempts to compile a given invocation.
        /// </summary>
        /// <param name="arguments">the argument expressions to compile</param>
        /// <param name="context">the context to compile in</param>
        /// <param name="typeHintHandler">a type hint handler to allow hinting</param>
        /// <param name="expr">the compiled <see cref="Expression"/></param>
        /// <returns><see langword="true"/> if the invocation successfully compiled, or
        /// <see langword="false"/> if it could not</returns>
        bool TryCompile(IReadOnlyList<MathExpression> arguments, 
            ICompilationContext<TSettings> context,
            ITypeHintHandler typeHintHandler, [MaybeNullWhen(false)] out Expression expr);
    }
}
