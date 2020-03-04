﻿using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MathExpr.Syntax.BinaryExpression;

namespace MathExpr.Compiler.OptimizationPasses
{
    public class BinaryExpressionCombinerPass : OptimizationPass<ICommutativitySettings>
    {
        public override MathExpression ApplyTo(BinaryExpression expr, ITransformContext<ICommutativitySettings> ctx)
        {
            var list = expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList();
            if (!ctx.Settings.IgnoreCommutativityFor.Contains(expr.Type))
            {
                switch (expr.Type)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Multiply:
                    case ExpressionType.And:
                    case ExpressionType.Or:

                        bool IsCombinableExpression(MathExpression e)
                            => e is BinaryExpression ex && ex.Type == expr.Type;
                        if (list.Any(IsCombinableExpression))
                        {
                            foreach (var ex in list.ToArray().Where(IsCombinableExpression).Cast<BinaryExpression>())
                            {
                                list.Remove(ex);
                                list.AddRange(ex.Arguments);
                            }
                        }
                        break;
                }
            }
            return new BinaryExpression(expr.Type, list);
        }
    }
}
