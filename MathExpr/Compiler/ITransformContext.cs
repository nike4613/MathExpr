using System;
using System.Collections.Generic;
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
        public static DataProxy<TData> Data<TData>(this ITransformContext ctx)
            => new DataProxy<TData>(ctx);

        public struct DataProxy<TData>
        {
            private readonly ITransformContext context;
            internal DataProxy(ITransformContext ctx) => context = ctx;
            public TData GetIn<TScope>() => context.GetOrCreateData<TScope, TData>(() => default!);
            public TData GetIn<TScope>(TScope _) => GetIn<TScope>();
            public TData GetOrCreateIn<TScope>(Func<TData> creator) => context.GetOrCreateData<TScope, TData>(creator);
            public TData GetOrCreateIn<TScope>(TScope _, Func<TData> creator) => GetOrCreateIn<TScope>(creator);
            public TData GetOrCreateIn<TScope>(TData defaultValue) => GetOrCreateIn<TScope>(() => defaultValue);
            public TData GetOrCreateIn<TScope>(TScope _, TData defaultValue) => GetOrCreateIn<TScope>(defaultValue);
            public TData GetOrCreateIn<TScope>() => GetOrCreateIn<TScope>(() => default!);
            public TData GetOrCreateIn<TScope>(TScope _) => GetOrCreateIn<TScope>();
            public void SetIn<TScope>(TData value) => context.SetData<TScope, TData>(value);
            public void SetIn<TScope>(TScope _, TData value) => SetIn<TScope>(value);
        }
    }
}
