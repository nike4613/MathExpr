using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace MathExpr.Compiler.Compilation
{
    public interface ICompilationTransformContext<out TSettings> : ITransformContext<TSettings, MathExpression, Expression>
    { }

    public static class CompilationTransformContext
    {
        public static CompilationTransformContext<TSettings> CreateWith<TSettings>(TSettings settings, ICompilationTransformPass<TSettings> pass)
            => new CompilationTransformContext<TSettings>(settings, pass);
    }

    public class CompilationTransformContext<TSettings>
    {
        public ICompilationTransformPass<TSettings> Transformer { get; }

        public CompilationTransformContext(TSettings settings, ICompilationTransformPass<TSettings> transform)
        {
            Settings = settings;
            Transformer = transform;
        }

        public TSettings Settings { get; }

        private class ContextImpl : DataProvidingTransformContext, ICompilationTransformContext<TSettings>
        {
            private readonly CompilationTransformContext<TSettings> owner;
            public ContextImpl(CompilationTransformContext<TSettings> own) : base(null)
                => owner = own;

            public TSettings Settings => owner.Settings;
            public Expression Transform(MathExpression from)
                => owner.Transformer.ApplyTo(from, this);
        }
        public Expression Transform(MathExpression from)
            => new ContextImpl(this).Transform(from);
    }
}
