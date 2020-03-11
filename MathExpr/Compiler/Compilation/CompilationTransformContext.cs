using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    public interface ICompilationTransformContext<out TSettings, TTo> : ITransformContext<TSettings, MathExpression, TTo>
    { }

    public static class CompilationTransformContext
    {
        public static CompilationTransformContext<TSettings, TTo> CreateWith<TSettings, TTo>(TSettings settings, ICompilationTransformPass<TSettings, TTo> pass)
            => new CompilationTransformContext<TSettings, TTo>(settings, pass);
    }

    public class CompilationTransformContext<TSettings, TTo> : DataProvidingTransformContext, ICompilationTransformContext<TSettings, TTo>
    {
        public ICompilationTransformPass<TSettings, TTo> Transformer { get; }

        public CompilationTransformContext(TSettings settings, ICompilationTransformPass<TSettings, TTo> transform) : base(null)
        {
            Settings = settings;
            Transformer = transform;
        }

        public TSettings Settings { get; }

        public TTo Transform(MathExpression from)
            => Transformer.ApplyTo(from, this);
    }
}
