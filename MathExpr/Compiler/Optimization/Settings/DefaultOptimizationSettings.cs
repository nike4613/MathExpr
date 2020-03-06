using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    public class DefaultOptimizationSettings : IDomainRestrictionSettings, ICommutativitySettings, IFunctionInlineSettings
    {
        public bool IgnoreDomainRestrictions { get; set; } = false;
        public bool AllowDomainChangingOptimizations { get; set; } = true;
        public IList<MathExpression> DomainRestrictions { get; } = new List<MathExpression>();

        public IList<BinaryExpression.ExpressionType> IgnoreCommutativityFor { get; } = new List<BinaryExpression.ExpressionType>();

        public bool ShouldInline { get; set; } = true;
        public int DoNotInlineAfterSize { get; set; } = int.MaxValue;
    }
}
