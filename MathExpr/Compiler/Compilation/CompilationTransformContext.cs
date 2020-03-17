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

    public class CompilationTransformContext<TSettings> : DataProvidingTransformContext, ICompilationTransformContext<TSettings>
    {
        public ICompilationTransformPass<TSettings> Transformer { get; }

        public CompilationTransformContext(TSettings settings, ICompilationTransformPass<TSettings> transform) : base(null)
        {
            Settings = settings;
            Transformer = transform;
        }

        public TSettings Settings { get; }

        public Expression Transform(MathExpression from)
            => Transformer.ApplyTo(from, this);
    }
}
