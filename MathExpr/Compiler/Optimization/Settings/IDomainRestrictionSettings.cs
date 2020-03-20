using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    /// <summary>
    /// A settings interface for managing domain restrictions.
    /// </summary>
    public interface IDomainRestrictionSettings
    {
        // TODO: pull this out to somewhere that makes more sense to be transferred to compiler
        /// <summary>
        /// Whether or not the compiler should ignore domain restrictions.
        /// </summary>
        public bool IgnoreDomainRestrictions { get; }
        /// <summary>
        /// Whether or not to allow optimization passes to make domain changing optimizations.
        /// </summary>
        public bool AllowDomainChangingOptimizations { get; }
        // TODO: replace this with appropriately scoped context data
        /// <summary>
        /// A list of expressions that, when truthy, indicate that the function is not defined.
        /// </summary>
        public IList<MathExpression> DomainRestrictions { get; }
    }
}
