using MathExpr.Syntax;
using MathExpr.Utilities;
using MathExpr.Compiler.Optimization.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathExpr.Compiler.Compilation.Builtins;

namespace MathExpr.Compiler.Optimization.Passes
{
    /// <summary>
    /// An optimization pass that precomputes the value of a constant expression of the form <c>exp(c)</c> or <c>ln(c)</c>.
    /// </summary>
    public class BuiltinExponentConstantReductionPass : OptimizationPass<object?>
    {
        /// <inheritdoc/>
        public override MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<object?> ctx, out bool transformResult)
        {
            if (!expr.IsUserDefined && (expr.Name == BuiltinFunctionExp.ConstName || expr.Name == BuiltinFunctionLn.ConstName))
            { // exp(x)
                if (expr.Arguments.Count == 1)
                {
                    var arg = ApplyTo(expr.Arguments.First(), ctx);
                    if (arg is LiteralExpression lit)
                    {
                        transformResult = true;
                        if (expr.Name == BuiltinFunctionExp.ConstName)
                            return new LiteralExpression(DecimalMath.Exp(lit.Value)).WithToken(expr.Token);
                        if (expr.Name == BuiltinFunctionLn.ConstName)
                            return new LiteralExpression(DecimalMath.Ln(lit.Value)).WithToken(expr.Token);
                    }
                }
            }

            return base.ApplyTo(expr, ctx, out transformResult);
        }
    }
}
