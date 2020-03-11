using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    public class DefaultBasicCompileToLinqExpressionSettings : ICompileToLinqExpressionSettings
    {
        public Type ExpectReturn { get; set; } = typeof(decimal);

        public IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; } = new Dictionary<VariableExpression, ParameterExpression>();

        public bool IgnoreDomainRestrictions { get; set; } = false;

        public bool AllowDomainChangingOptimizations { get; set; } = true;

        public IList<MathExpression> DomainRestrictions { get; } = new List<MathExpression>();
    }
}
