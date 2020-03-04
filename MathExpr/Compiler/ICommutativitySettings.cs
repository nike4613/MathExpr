using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public interface ICommutativitySettings
    {
        IList<BinaryExpression.ExpressionType> IgnoreCommutativityFor { get; }
    }
}
