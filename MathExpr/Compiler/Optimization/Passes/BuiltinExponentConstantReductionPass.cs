using MathExpr.Syntax;
using MathExpr.Utilities;
using MathExpr.Compiler.Optimization.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization.Passes
{
    public class BuiltinExponentConstantReductionPass : OptimizationPass<object?>
    {
        public override MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<object?> ctx, out bool transformResult)
        {
            if (!expr.IsPrime && (expr.Name == FunctionExpression.ExpName || expr.Name == FunctionExpression.LnName))
            { // exp(x)
                if (expr.Arguments.Count == 1)
                {
                    var arg = ApplyTo(expr.Arguments.First(), ctx);
                    if (arg is LiteralExpression lit)
                    {
                        transformResult = true;
                        if (expr.Name == FunctionExpression.ExpName)
                            return new LiteralExpression(DecimalMath.Exp(lit.Value));
                        if (expr.Name == FunctionExpression.LnName)
                            return new LiteralExpression(DecimalMath.Ln(lit.Value));
                    }
                }
            }

            return base.ApplyTo(expr, ctx, out transformResult);
        }
    }
}
