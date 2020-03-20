using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MathExpr.Compiler
{
    public abstract class DataProvidingTransformContext : IDataContext
    {
        private readonly DataProvidingTransformContext? parent;
        protected readonly Dictionary<(Type scope, Type type), object?> DataStore = new Dictionary<(Type, Type), object?>();

        protected DataProvidingTransformContext(DataProvidingTransformContext? parent = null)
            => this.parent = parent;

        private static class DataStoreKeyStore<TScope, TData>
        {
            public static (Type scope, Type type) Key = (typeof(TScope), typeof(TData));
        }

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

        protected TData AddData<TScope, TData>(TData data)
        {
            var key = DataStoreKeyStore<TScope, TData>.Key;
            if (DataStore.ContainsKey(key))
                DataStore[key] = data;
            else
                DataStore.Add(key, data);
            return data;
        }

        public TData GetOrCreateData<TScope, TData>(Func<TData> creator)
        {
            if (TryGetData<TScope, TData>(out var data))
                return data;
            else
                return AddData<TScope, TData>(creator());
        }

        public void SetData<TScope, TData>(TData data)
            => AddData<TScope, TData>(data);
    }
}
