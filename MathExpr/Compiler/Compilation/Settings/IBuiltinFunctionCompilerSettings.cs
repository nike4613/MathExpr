using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    public interface IBuiltinFunctionCompilerSettings<TSettings>
    {
        IDictionary<(string name, int argcount), IBuiltinFunction<TSettings>> BuiltinFunctions { get; }
    }

    public static class IBuiltinFunctionCompilerSettingsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddBuiltin<TSettings>(this IBuiltinFunctionCompilerSettings<TSettings> self, IBuiltinFunction<TSettings> function)
            => self.BuiltinFunctions.Add((function.Name, function.ParamCount), function);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddBuiltinProxy<TSettings> AddBuiltin<TSettings>(this IBuiltinFunctionCompilerSettings<TSettings> self)
            => new AddBuiltinProxy<TSettings>(self);

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Only used as a proxy here, that can be inlined well.")]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Should never be explicitly stored or compared.")]
        public struct AddBuiltinProxy<TSettings>
        {
            private readonly IBuiltinFunctionCompilerSettings<TSettings> settings;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal AddBuiltinProxy(IBuiltinFunctionCompilerSettings<TSettings> s)
                => settings = s;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OfType<TFunction>() where TFunction : IBuiltinFunction<TSettings>, new()
                => settings.AddBuiltin(new TFunction());
        }
    }
}
