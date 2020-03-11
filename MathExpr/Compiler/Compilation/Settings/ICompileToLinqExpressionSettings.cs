using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    public interface ICompileToLinqExpressionSettings : IDomainRestrictionSettings
    {
        Type ExpectReturn { get; }
        IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; }
    }
}
