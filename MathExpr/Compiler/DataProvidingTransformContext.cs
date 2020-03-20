using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MathExpr.Compiler
{
    /// <summary>
    /// A basic implementation of <see cref="IDataContext"/>, providing a nested dictionary based data storage.
    /// </summary>
    public abstract class DataProvidingTransformContext : IDataContext
    {
        private readonly DataProvidingTransformContext? parent;
        /// <summary>
        /// The data storage dictionary used to store the data in this context.
        /// </summary>
        protected readonly Dictionary<(Type scope, Type type), object?> DataStore = new Dictionary<(Type, Type), object?>();

        /// <summary>
        /// Constructs a new context, optionally with a parent context.
        /// </summary>
        /// <param name="parent">the parent context</param>
        protected DataProvidingTransformContext(DataProvidingTransformContext? parent = null)
            => this.parent = parent;

        private static class DataStoreKeyStore<TScope, TData>
        {
            public static (Type scope, Type type) Key = (typeof(TScope), typeof(TData));
        }

        /// <summary>
        /// Attempts to get the data associated with a given type and scope.
        /// </summary>
        /// <remarks>
        /// If the data is not found in this context's data store, the lookup is then forwarded to this
        /// context's parent, if it has one.
        /// </remarks>
        /// <typeparam name="TScope">the scope type that the requested data is associated with</typeparam>
        /// <typeparam name="TData">the type of the data to retrieve</typeparam>
        /// <param name="data">the data that was stored, if it existed</param>
        /// <returns><see langword="true"/> if the data was found either in this context or a parent, 
        /// <see langword="false"/> otherwise</returns>
        protected bool TryGetData<TScope, TData>([MaybeNullWhen(false)] out TData data)
        {
            if (DataStore.TryGetValue(DataStoreKeyStore<TScope, TData>.Key, out var val))
            {
                data = (TData)val!;
                return true;
            }
            else
            {
                if (parent != null)
                    return parent.TryGetData<TScope, TData>(out data);
                else
                {
                    data = default!;
                    return false;
                }
            }
        }

        /// <summary>
        /// Adds or sets data in this context, assocaited with a scope.
        /// </summary>
        /// <typeparam name="TScope">the scope type that the requested data is associated with</typeparam>
        /// <typeparam name="TData">the type of the data to retrieve</typeparam>
        /// <param name="data">the data to store</param>
        /// <returns><paramref name="data"/></returns>
        protected TData AddData<TScope, TData>(TData data)
        {
            var key = DataStoreKeyStore<TScope, TData>.Key;
            if (DataStore.ContainsKey(key))
                DataStore[key] = data;
            else
                DataStore.Add(key, data);
            return data;
        }

        /// <summary>
        /// Gets the data in the given scope, if present, otherwise creating it with <paramref name="creator"/>.
        /// </summary>
        /// <typeparam name="TScope">the scope type that the requested data is associated with</typeparam>
        /// <typeparam name="TData">the type of the data to retrieve</typeparam>
        /// <param name="creator">a delegate to use to create the data if it does not exist</param>
        /// <returns>the found or created data</returns>
        public TData GetOrCreateData<TScope, TData>(Func<TData> creator)
        {
            if (TryGetData<TScope, TData>(out var data))
                return data;
            else
                return AddData<TScope, TData>(creator());
        }

        /// <summary>
        /// Sets a data value in a given scope in this context.
        /// </summary>
        /// <typeparam name="TScope">the scope type that the requested data is associated with</typeparam>
        /// <typeparam name="TData">the type of the data to retrieve</typeparam>
        /// <param name="data">the data to set</param>
        public void SetData<TScope, TData>(TData data)
            => AddData<TScope, TData>(data);
    }
}
