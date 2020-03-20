using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    /// <summary>
    /// A transformation pass.
    /// </summary>
    /// <typeparam name="TContext">the context type that the implementation expects</typeparam>
    /// <typeparam name="TFrom">the source object type</typeparam>
    /// <typeparam name="TTo">the target object type</typeparam>
    public interface ITransformPass<in TContext, TFrom, TTo>
    {
        /// <summary>
        /// Applies this pass to the given object.
        /// </summary>
        /// <param name="obj">the object to transform</param>
        /// <param name="context">the context that the transformation is taking place in</param>
        /// <returns>the transformed object</returns>
        TTo ApplyTo(TFrom obj, TContext context);
    }
}
