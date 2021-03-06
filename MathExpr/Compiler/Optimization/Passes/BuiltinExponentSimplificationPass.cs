﻿using MathExpr.Syntax;
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
    /// An optimization pass that reduces expressions of the form <c>exp(ln(x))</c> into <c>x</c> with a domain  restriction.
    /// </summary>
    public class BuiltinExponentSimplificationPass : OptimizationPass<IDomainRestrictionSettings>
    {
        /// <inheritdoc/>
        public override MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<IDomainRestrictionSettings> ctx, out bool transformResult)
        {
            switch (expr.Type)
            {
                case BinaryExpression.ExpressionType.Power:
                    if (expr.Left is LiteralExpression l && l.Value == DecimalMath.E)
                    {
                        transformResult = false; // because we will have already applied to our target
                        //  transform into call to `exp` function, then apply to that
                        return ApplyTo(new FunctionExpression(BuiltinFunctionExp.ConstName, new[] { ApplyTo(expr.Right, ctx) }.ToList(), false).WithToken(expr.Token), ctx);
                    }
                    break;
            }
            return base.ApplyTo(expr, ctx, out transformResult);
        }

        private ICollection<MathExpression> GetDomainRestrictions(IDataContext ctx)
            => DomainRestrictionSettings.GetDomainRestrictionsFor(ctx);

        /// <inheritdoc/>
        public override MathExpression ApplyTo(FunctionExpression f, IOptimizationContext<IDomainRestrictionSettings> ctx, out bool transformResult)
        {
            if (!f.IsUserDefined && f.Name == BuiltinFunctionExp.ConstName)
            { // exp(x)
                if (f.Arguments.Count == 1)
                {
                    var arg = f.Arguments.First();
                    if (arg is FunctionExpression fn && !fn.IsUserDefined && fn.Name == BuiltinFunctionLn.ConstName)
                    { // exp(ln(x))
                        if (fn.Arguments.Count == 1 && ctx.Settings.AllowDomainChangingOptimizations)
                        {
                            var ln = fn.Arguments.First();
                            var dr = GetDomainRestrictions(ctx);
                            dr.Add(new BinaryExpression(ln, new LiteralExpression(0), BinaryExpression.ExpressionType.LessEq));
                            transformResult = false; //because we will have already applied to it
                            return ApplyTo(ln, ctx); // apply to the argument, transform to that
                        }
                    }
                }
            }
            return base.ApplyTo(f, ctx, out transformResult);
        }
    }
}
