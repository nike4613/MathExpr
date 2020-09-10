using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathExpr.Compiler
{
    /// <summary>
    /// A typed transformation context.
    /// </summary>
    /// <typeparam name="TSettings">the type of settings to provide</typeparam>
    /// <typeparam name="TFrom">the type that is being transformed from</typeparam>
    /// <typeparam name="TTo">the type that is being transformed to</typeparam>
    public interface ITransformContext<out TSettings, TFrom, TTo> : IDataContext
    {
        /// <summary>
        /// The settings associated with the current context.
        /// </summary>
        TSettings Settings { get; }

        /// <summary>
        /// Transforms a <typeparamref name="TFrom"/> to a <typeparamref name="TTo"/>.
        /// </summary>
        /// <param name="from">the object to transform</param>
        /// <returns>the transformed object</returns>
        TTo Transform(TFrom from);
    }
    /// <summary>
    /// A data context that is capable of storing scoped data.
    /// </summary>
    public interface IDataContext
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
    /// <summary>
    /// Extensions for <see cref="IDataContext"/>.
    /// </summary>
    public static class DataContext
    {
        /// <summary>
        /// Gets a proxy to fluidly access typed data on a <see cref="IDataContext"/>.
        /// </summary>
        /// <typeparam name="TData">the type of data to access</typeparam>
        /// <param name="ctx">the data context to access</param>
        /// <returns>a proxy for fluidly accessing typed data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataProxy<TData> Data<TData>(this IDataContext ctx)
            => new DataProxy<TData>(ctx);

        /// <summary>
        /// A proxy object for accessing data fluidly on a <see cref="IDataContext"/>.
        /// </summary>
        /// <typeparam name="TData">the type of data to access</typeparam>
        public struct DataProxy<TData>
        {
            private readonly IDataContext context;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal DataProxy(IDataContext ctx) => context = ctx;
            /// <summary>
            /// Gets or creates a value using <paramref name="creator"/> in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="creator">a delegate that will be used to create the value if it does not exist</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(Func<TData> creator) => context.GetOrCreateData<TScope, TData>(creator);
            /// <summary>
            /// Gets or creates a value using <paramref name="creator"/> in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <param name="creator">a delegate that will be used to create the value if it does not exist</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _, Func<TData> creator) => GetOrCreateIn<TScope>(creator);
            /// <summary>
            /// Gets or adds a value using <paramref name="defaultValue"/> as a default in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="defaultValue">the default value to use if it doesn't exist</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TData defaultValue) => GetOrCreateIn<TScope>(() => defaultValue);
            /// <summary>
            /// Gets or adds a value using <paramref name="defaultValue"/> as a default in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <param name="defaultValue">the default value to use if it doesn't exist</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _, TData defaultValue) => GetOrCreateIn<TScope>(defaultValue);
            /// <summary>
            /// Gets or creates a default constructed value in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>() => GetOrCreateIn<TScope>(Activator.CreateInstance<TData>);
            /// <summary>
            /// Gets or creates a default constructed value in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrCreateIn<TScope>(TScope _) => GetOrCreateIn<TScope>();
            /// <summary>
            /// Gets or creates the <see langword="default"/> of a value in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrDefaultIn<TScope>() => GetOrCreateIn<TScope>(() => default!);
            /// <summary>
            /// Gets or creates the <see langword="default"/> of a value in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <returns>the data</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetOrDefaultIn<TScope>(TScope _) => GetOrDefaultIn<TScope>();
            /// <summary>
            /// Gets a value in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <returns>the data</returns>
            /// <exception cref="InvalidOperationException">if the value does not exist in the scope</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetIn<TScope>() => GetOrCreateIn<TScope>(() => throw new InvalidOperationException("Value did not exist in scope"));
            /// <summary>
            /// Gets a value in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to find the data in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <returns>the data</returns>
            /// <exception cref="InvalidOperationException">if the value does not exist in the scope</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TData GetIn<TScope>(TScope _) => GetIn<TScope>();

            /// <summary>
            /// Sets a value in scope <typeparamref name="TScope"/>.
            /// </summary>
            /// <typeparam name="TScope">the scope to set the value in</typeparam>
            /// <param name="value">the value to set</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetIn<TScope>(TData value) => context.SetData<TScope, TData>(value);
            /// <summary>
            /// Sets a value in a type-deduced scope.
            /// </summary>
            /// <typeparam name="TScope">the scope to set the value in</typeparam>
            /// <param name="_">used to deduce the scope</param>
            /// <param name="value">the value to set</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetIn<TScope>(TScope _, TData value) => SetIn<TScope>(value);
        }
    }
}
