using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using MathExpr.Compiler.Compilation.Settings;
using System.Linq;

namespace MathExpr.Compiler.Compilation
{
    /// <summary>
    /// A specialization of <see cref="ITransformContext{TSettings, TFrom, TTo}"/> for transforming a 
    /// <see cref="MathExpression"/> to an <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSettings">the settings type to provide</typeparam>
    public interface ICompilationTransformContext<out TSettings> : ITransformContext<TSettings, MathExpression, Expression>
    { }

    /// <summary>
    /// A static class for creating a <see cref="CompilationTransformContext{TSettings}"/> easily, using type inference.
    /// </summary>
    public static class CompilationTransformContext
    {
        /// <summary>
        /// Creates a <see cref="CompilationTransformContext{TSettings}"/> using the specified settings and transformer.
        /// </summary>
        /// <typeparam name="TSettings">the settings type to provide to the transformer</typeparam>
        /// <param name="settings">the settings to create the context with</param>
        /// <param name="pass">the compilation backend to use</param>
        /// <returns>the new <see cref="CompilationTransformContext{TSettings}"/></returns>
        public static CompilationTransformContext<TSettings> CreateWith<TSettings>(TSettings settings, ICompilationTransformPass<TSettings> pass)
            => new CompilationTransformContext<TSettings>(settings, pass);

        /// <summary>
        /// Creates a <see cref="CompilationTransformContext{TSettings}"/> using the specified settings, builtin functions, and transformer.
        /// </summary>
        /// <typeparam name="TSettings">the settings type to provide to the transformer</typeparam>
        /// <param name="settings">the settings to create the context with</param>
        /// <param name="pass">the compilation backend to use</param>
        /// <param name="builtinFunctions">the builtin functions to use</param>
        /// <returns>the new <see cref="CompilationTransformContext{TSettings}"/></returns>
        public static CompilationTransformContext<TSettings> CreateWith<TSettings>(TSettings settings, ICompilationTransformPass<TSettings> pass,
            IEnumerable<IBuiltinFunction<TSettings>> builtinFunctions)
            where TSettings : IBuiltinFunctionWritableCompilerSettings<TSettings>
        {
            foreach (var fun in builtinFunctions)
                settings.AddBuiltin(fun);
            return CreateWith(settings, pass);
        }
        /// <summary>
        /// Creates a <see cref="CompilationTransformContext{TSettings}"/> using the specified settings, builtin functions, and transformer.
        /// </summary>
        /// <typeparam name="TSettings">the settings type to provide to the transformer</typeparam>
        /// <param name="settings">the settings to create the context with</param>
        /// <param name="pass">the compilation backend to use</param>
        /// <param name="builtinFunctions">the builtin functions to use</param>
        /// <returns>the new <see cref="CompilationTransformContext{TSettings}"/></returns>
        public static CompilationTransformContext<TSettings> CreateWith<TSettings>(TSettings settings, ICompilationTransformPass<TSettings> pass,
            params IBuiltinFunction<TSettings>[] builtinFunctions)
            where TSettings : IBuiltinFunctionWritableCompilerSettings<TSettings>
            => CreateWith(settings, pass, builtinFunctions.AsEnumerable());
    }

    /// <summary>
    /// A context for managing the compilation of <see cref="MathExpression"/>s.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public class CompilationTransformContext<TSettings> : DataProvidingContext, ICompilationTransformContext<TSettings>
    {
        /// <summary>
        /// The <see cref="ICompilationTransformContext{TSettings}"/> to use to compile expressions.
        /// </summary>
        public ICompilationTransformPass<TSettings> Transformer { get; }

        /// <summary>
        /// Creates a new context with the given settings and transformer.
        /// </summary>
        /// <param name="settings">the settings to create the context with</param>
        /// <param name="transform">the compilation backend to use</param>
        public CompilationTransformContext(TSettings settings, ICompilationTransformPass<TSettings> transform)
        {
            Settings = settings;
            Transformer = transform;
        }

        /// <summary>
        /// The settings object provided to the compiler backend.
        /// </summary>
        public TSettings Settings { get; }

        /// <summary>
        /// Sets this context's parent data context.
        /// </summary>
        /// <param name="newParent">the context to parent this to</param>
        public void SetParentDataContext(DataProvidingContext? newParent) => SetParent(newParent);

        private class ContextImpl : DataProvidingContext, ICompilationTransformContext<TSettings>
        {
            private readonly CompilationTransformContext<TSettings> owner;
            public ContextImpl(CompilationTransformContext<TSettings> own) : base(own)
                => owner = own;

            public TSettings Settings => owner.Settings;
            public Expression Transform(MathExpression from)
                => owner.Transformer.ApplyTo(from, this);
        }

        /// <summary>
        /// Transforms a given <see cref="MathExpression"/> into an equivalent <see cref="Expression"/> implementation.
        /// </summary>
        /// <param name="from">the expression to transform</param>
        /// <returns>an implementation of that expression</returns>
        public Expression Transform(MathExpression from)
            => new ContextImpl(this).Transform(from);
    }
}
