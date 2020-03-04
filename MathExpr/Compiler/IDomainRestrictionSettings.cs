using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public interface IDomainRestrictionSettings
    {
        public bool IgnoreDomainRestrictions { get; }
        public bool AllowDomainChangingOptimizations { get; }
        public IList<MathExpression> DomainRestrictions { get; }
    }
}
