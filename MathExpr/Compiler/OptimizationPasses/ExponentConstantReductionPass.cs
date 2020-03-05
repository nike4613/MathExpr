using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.OptimizationPasses
{
    public class ExponentConstantReductionPass : OptimizationPass<object?>
    {
        public override MathExpression ApplyTo(FunctionExpression expr, ITransformContext<object?> ctx)
        {
            if (!expr.IsPrime && (expr.Name == FunctionExpression.ExpName || expr.Name == FunctionExpression.LnName))
            { // exp(x)
                if (expr.Arguments.Count == 1)
                {
                    var arg = ApplyTo(expr.Arguments.First(), ctx);
                    if (arg is LiteralExpression lit)
                    {
                        if (expr.Name == FunctionExpression.ExpName)
                            return new LiteralExpression(DecimalMath.Exp(lit.Value));
                        if (expr.Name == FunctionExpression.LnName)
                            return new LiteralExpression(DecimalMath.Ln(lit.Value));
                    }
                }
            }

            return base.ApplyTo(expr, ctx);
        }
    }
}
