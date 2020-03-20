using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    /// <summary>
    /// A settings interface that allows compiling with <see cref="IBuiltinFunction{TSettings}"/>.
    /// </summary>
    /// <typeparam name="TSettings">the aggregate settings type that the builtins require</typeparam>
    public interface IBuiltinFunctionCompilerSettings<TSettings>
    {
        /// <summary>
        /// A dictionary of a builtin name to a list of implementations.
        /// </summary>
        IDictionary<string, IList<IBuiltinFunction<TSettings>>> BuiltinFunctions { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IBuiltinFunctionCompilerSettings{TSettings}"/>.
    /// </summary>
    public static class IBuiltinFunctionCompilerSettingsExtensions
    {
        /// <summary>
        /// Adds a builtin implementation to a given settings object.
        /// </summary>
        /// <typeparam name="TSettings">the settings type the builtin requires</typeparam>
        /// <param name="self">the settings object to add to</param>
        /// <param name="function">the builtin implementation to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddBuiltin<TSettings>(this IBuiltinFunctionCompilerSettings<TSettings> self, IBuiltinFunction<TSettings> function)
        {
            if (!self.BuiltinFunctions.TryGetValue(function.Name, out var list))
                self.BuiltinFunctions.Add(function.Name, list = new List<IBuiltinFunction<TSettings>>());
            list.Add(function);
        }
        /// <summary>
        /// Allows a fluid syntax to add a simple default-constructed typed builtin.
        /// </summary>
        /// <typeparam name="TSettings">the settings type for the settings object</typeparam>
        /// <param name="self">the settings object to add to</param>
        /// <returns>a proxy that allows fluid syntax to add a simple typed builtin</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddBuiltinProxy<TSettings> AddBuiltin<TSettings>(this IBuiltinFunctionCompilerSettings<TSettings> self)
            => new AddBuiltinProxy<TSettings>(self);

        /// <summary>
        /// A proxy type used for fluid syntax.
        /// </summary>
        /// <typeparam name="TSettings">the settings type for the settings object</typeparam>
        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Only used as a proxy here, that can be inlined well.")]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Should never be explicitly stored or compared.")]
        public struct AddBuiltinProxy<TSettings>
        {
            private readonly IBuiltinFunctionCompilerSettings<TSettings> settings;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal AddBuiltinProxy(IBuiltinFunctionCompilerSettings<TSettings> s)
                => settings = s;
            /// <summary>
            /// Adds a builtin of type <typeparamref name="TFunction"/> to the proxied settings object.
            /// </summary>
            /// <typeparam name="TFunction">the type of the function implementation</typeparam>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OfType<TFunction>() where TFunction : IBuiltinFunction<TSettings>, new()
                => settings.AddBuiltin(new TFunction());
        }
    }
}
