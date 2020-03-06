using MathExpr.Compiler.Optimization.Passes;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler
{
    public static class OptimizationContext
    {
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, IEnumerable<IOptimizationPass<TSettings>> passes)
            => new OptimizationContext<TSettings>(passes, settings);
        public static OptimizationContext<TSettings> CreateWith<TSettings>(TSettings settings, params IOptimizationPass<TSettings>[] passes)
            => new OptimizationContext<TSettings>(passes, settings);
    }

    public interface IOptimizationContext<out TSettings> : ITransformContext<TSettings, MathExpression, MathExpression>
    {
    }

    public class OptimizationContext<TSettings>
    {
        private readonly List<IOptimizationPass<TSettings>> passes;

        public IEnumerable<IOptimizationPass<TSettings>> Passes => passes;

        public TSettings Settings { get; set; }

        public OptimizationContext(IEnumerable<IOptimizationPass<TSettings>> passes, TSettings settings)
        {
            this.passes = passes.ToList();
            Settings = settings;
        }

        private class SubContext : IOptimizationContext<TSettings>
        {
            private readonly OptimizationContext<TSettings> owner;
            private readonly SubContext? parent = null;
            private readonly int currentIndex;
            private readonly Dictionary<(Type scope, Type type), object?> dataStore = new Dictionary<(Type, Type), object?>();

            public SubContext(OptimizationContext<TSettings> own, int index = 0)
            {
                owner = own;
                currentIndex = index;
            }
            private SubContext(SubContext parent) : this(parent.owner, parent.currentIndex + 1)
            {
                this.parent = parent;
            }

            public TSettings Settings => owner.Settings;

            private SubContext? _child = null;
            private SubContext Child
            {
                get
                {
                    if (_child == null)
                        _child = new SubContext(this);
                    else
                        _child.dataStore.Clear();
                    return _child;
                }
            }

            public MathExpression Transform(MathExpression from)
            {
                if (currentIndex >= owner.passes.Count)
                    return from;
                else
                    return owner.passes[currentIndex].ApplyTo(from, Child);
            }

            private static class DataStoreKeyStore<TScope, TData>
            {
                public static (Type scope, Type type) Key = (typeof(TScope), typeof(TData));
            }

            private bool TryGetData<TScope, TData>(out TData data)
            {
                if (dataStore.TryGetValue(DataStoreKeyStore<TScope, TData>.Key, out var val))
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

            public TData GetOrCreateData<TScope, TData>(Func<TData> creator)
            {
                if (TryGetData<TScope, TData>(out var data))
                    return data;
                else
                {
                    var val = creator();
                    dataStore.Add(DataStoreKeyStore<TScope, TData>.Key, val);
                    return val;
                }
            }

            public void SetData<TScope, TData>(TData data)
            {
                var key = DataStoreKeyStore<TScope, TData>.Key;
                if (dataStore.ContainsKey(key))
                    dataStore[key] = data;
                else
                    dataStore.Add(key, data);
            }
        }

        /// <summary>
        /// Runs the expression through all optimization passes.
        /// </summary>
        /// <param name="expr">the expression to optimize</param>
        /// <returns>the optimized expression</returns>
        public MathExpression Optimize(MathExpression expr)
            => new SubContext(this).Transform(expr);
    }
}
