using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    /// <summary>
    /// A default implementation of all of the settings interfaces required by the default optimization passes.
    /// </summary>
    public class DefaultOptimizationSettings : IDomainRestrictionSettings, ICommutativitySettings, IFunctionInlineSettings
    {
        /// <inheritdoc/>
        public bool IgnoreDomainRestrictions { get; set; } = false;
        /// <inheritdoc/>
        public bool AllowDomainChangingOptimizations { get; set; } = true;
        /// <inheritdoc/>
        public IList<MathExpression> DomainRestrictions { get; } = new List<MathExpression>();

        /// <inheritdoc/>
        public IList<BinaryExpression.ExpressionType> IgnoreCommutativityFor { get; } = new List<BinaryExpression.ExpressionType>();

        /// <inheritdoc/>
        public bool ShouldInline { get; set; } = true;
        /// <inheritdoc/>
        public int DoNotInlineAfterSize { get; set; } = int.MaxValue;
    }
}
