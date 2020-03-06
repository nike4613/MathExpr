using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    public interface IDomainRestrictionSettings
    {
        public bool IgnoreDomainRestrictions { get; }
        public bool AllowDomainChangingOptimizations { get; }
        // TODO: replace this with appropriately scoped context data
        public IList<MathExpression> DomainRestrictions { get; }
    }
}
