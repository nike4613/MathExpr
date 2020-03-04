using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public class DefaultOptimizationSettings : IDomainRestrictionSettings, ICommutativitySettings
    {
        public bool IgnoreDomainRestrictions { get; set; } = false;
        public bool AllowDomainChangingOptimizations { get; set; } = true;
        public IList<MathExpression> DomainRestrictions { get; } = new List<MathExpression>();
        public IList<BinaryExpression.ExpressionType> IgnoreCommutativityFor { get; } = new List<BinaryExpression.ExpressionType>();
    }
}
