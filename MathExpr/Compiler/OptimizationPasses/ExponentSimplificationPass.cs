using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.OptimizationPasses
{
    public class ExponentSimplificationPass : OptimizationPass<IDomainRestrictionSettings>
    {
        public override MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<IDomainRestrictionSettings> ctx)
        {
            switch (expr.Type)
            {
                case BinaryExpression.ExpressionType.Exponent:
                    if (expr.Left is LiteralExpression l && l.Value == DecimalMath.E)
                    {
                        //  transform into call to `exp` function, then apply to that
                        return ApplyTo(new FunctionExpression(FunctionExpression.ExpName, new[] { ApplyTo(expr.Right, ctx) }.ToList(), false), ctx);
                    }
                    break;
            }
            return base.ApplyTo(expr, ctx);
        }
        public override MathExpression ApplyTo(FunctionExpression f, IOptimizationContext<IDomainRestrictionSettings> ctx)
        {
            if (!f.IsPrime && f.Name == FunctionExpression.ExpName)
            { // exp(x)
                if (f.Arguments.Count == 1)
                {
                    var arg = f.Arguments.First();
                    if (arg is FunctionExpression fn && !fn.IsPrime && fn.Name == FunctionExpression.LnName)
                    { // exp(ln(x))
                        if (fn.Arguments.Count == 1 && ctx.Settings.AllowDomainChangingOptimizations)
                        {
                            var ln = fn.Arguments.First();
                            ctx.Settings.DomainRestrictions.Add(new BinaryExpression(ln, BinaryExpression.ExpressionType.LessEq, new LiteralExpression(0)));
                            return ApplyTo(ln, ctx); // apply to the argument, transform to that
                        }
                    }
                }
            }
            return base.ApplyTo(f, ctx);
        }
    }
}
