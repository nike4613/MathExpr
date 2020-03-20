using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    /// <summary>
    /// Optimization settings regarding operation commutativity.
    /// </summary>
    public interface ICommutativitySettings
    {
        /// <summary>
        /// Selects which operation types to ignore commutativity for.
        /// </summary>
        /// <remarks>
        /// Ignoring commutativity can significantly reduce the number of optimizations that can be done,
        /// and so should be done with care, only when there is a type in play that is known to not
        /// behave normally for some set of operations.
        /// </remarks>
        IList<BinaryExpression.ExpressionType> IgnoreCommutativityFor { get; }
    }
}
