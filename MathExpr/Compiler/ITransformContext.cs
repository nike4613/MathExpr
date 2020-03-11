using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathExpr.Compiler
{
    public interface ITransformContext<out TSettings, TFrom, TTo> : ITransformContext
    {
        TSettings Settings { get; }

        TTo Transform(TFrom from);
    }
    public interface ITransformContext
    {
        /// <summary>
        /// Gets or creates a scoped data type in the context, using the specified creator.
        /// </summary>
        /// <typeparam name="TScope">the type to use as a scope</typeparam>
        /// <typeparam name="TData">the type of data to get</typeparam>
        /// <param name="creator">the creator for when the data type doesn't exist</param>
        /// <returns>the data value</returns>
        TData GetOrCreateData<TScope, TData>(Func<TData> creator);
        /// <summary>
        /// Sets a data type in the context, using a type for scope.
        /// </summary>
        /// <typeparam name="TScope">the type to use as a scope</typeparam>
        /// <typeparam name="TData">the type of data to set</typeparam>
        /// <param name="data">the value to set</param>
        void SetData<TScope, TData>(TData data);
    }

    public static class TranformContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataProxy<TData> Data<TData>(this ITransformContext ctx)
            => new DataProxy<TData>(ctx);

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Only used as a proxy here, that can be inlined well.")]
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Should never be explicitly stored or compared.")]
        public struct DataProxy<TData>
        {
            private readonly ITransformContext context;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal DataProxy(ITransformContext ctx) => context = ctx;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(Func<TData> creator) => context.GetOrCreateData<TScope, TData>(creator);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _, Func<TData> creator) => GetOrCreateIn<TScope>(creator);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TData defaultValue) => GetOrCreateIn<TScope>(() => defaultValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _, TData defaultValue) => GetOrCreateIn<TScope>(defaultValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>() => GetOrCreateIn<TScope>(Activator.CreateInstance<TData>);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _) => GetOrCreateIn<TScope>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrDefaultIn<TScope>() => GetOrCreateIn<TScope>(() => default!);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrDefaultIn<TScope>(TScope _) => GetOrDefaultIn<TScope>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetIn<TScope>(TData value) => context.SetData<TScope, TData>(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetIn<TScope>(TScope _, TData value) => SetIn<TScope>(value);
        }
    }
}
