using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace MathExpr.Compiler.Compilation
{
    /// <summary>
    /// A specialization of <see cref="ITransformPass{TContext, TFrom, TTo}"/> taking a <see cref="ICompilationContext{TSettings}"/>
    /// and transforming a <see cref="MathExpression"/> to an <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSettings">the settings type that the implementer requires.</typeparam>
    public interface ICompiler<in TSettings>
           : ITransformPass<ICompilationContext<TSettings>, MathExpression, Expression>
    {
    }

    /// <summary>
    /// A basic implementation of <see cref="ICompiler{TSettings}"/> that provides overload access to each type of
    /// <see cref="MathExpression"/>.
    /// </summary>
    /// <typeparam name="TSettings">the settings type the implementer requires</typeparam>
    public abstract class Compiler<TSettings> : ICompiler<TSettings>
    {
        /// <summary>
        /// The core application method, that forwards to the overloads.
        /// </summary>
        /// <param name="expr">the expression to apply the transform pass to</param>
        /// <param name="ctx">the context to apply in</param>
        /// <returns>the <see cref="Expression"/> that was a result of transforming <paramref name="expr"/></returns>
        /// <seealso cref="ApplyTo(Syntax.UnaryExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(Syntax.BinaryExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(Syntax.MemberExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(VariableExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(FunctionExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(LiteralExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(StringExpression, ICompilationContext{TSettings})"/>
        /// <seealso cref="ApplyTo(CustomDefinitionExpression, ICompilationContext{TSettings})"/>
        public virtual Expression ApplyTo(MathExpression expr, ICompilationContext<TSettings> ctx)
            => expr switch
            {
                Syntax.BinaryExpression b => ApplyTo(b, ctx),
                Syntax.UnaryExpression b => ApplyTo(b, ctx),
                Syntax.MemberExpression b => ApplyTo(b, ctx),
                VariableExpression b => ApplyTo(b, ctx),
                FunctionExpression b => ApplyTo(b, ctx),
                LiteralExpression b => ApplyTo(b, ctx),
                StringExpression b => ApplyTo(b, ctx),
                CustomDefinitionExpression b => ApplyTo(b, ctx),
                _ => throw new ArgumentException("Unknown expression type", nameof(expr))
            };

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public abstract Expression ApplyTo(Syntax.BinaryExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(Syntax.UnaryExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(Syntax.MemberExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(VariableExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(FunctionExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(LiteralExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(StringExpression expr, ICompilationContext<TSettings> ctx);
        public abstract Expression ApplyTo(CustomDefinitionExpression expr, ICompilationContext<TSettings> ctx);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
