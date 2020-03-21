using MathExpr.Syntax;
using MathExpr.Utilities;
using MathExpr.Compiler.Optimization.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization.Passes
{
    /// <summary>
    /// An optimization pass that precomputes the value of some constant operation.
    /// </summary>
    public class LiteralCombinerPass : OptimizationPass
    {
        /// <inheritdoc/>
        public override MathExpression ApplyTo(BinaryExpression expr, IOptimizationContext<object?> ctx, out bool transformResult)
        {
            transformResult = true;
            var list = expr.Arguments.Select(e => ApplyTo(e, ctx)).ToList();
            static bool IsValueExpression(MathExpression e)
                => e is LiteralExpression;
            if (list.Count(IsValueExpression) > 1)
            {
                var arr = list.Where(IsValueExpression).Cast<LiteralExpression>().ToArray();
                var sum = arr.Select(l => l.Value).Aggregate(expr.Type switch
                {
                    BinaryExpression.ExpressionType.Add => (a, b) => a + b,
                    BinaryExpression.ExpressionType.Subtract => (a, b) => a - b,
                    BinaryExpression.ExpressionType.Multiply => (a, b) => a * b,
                    BinaryExpression.ExpressionType.Divide => (a, b) => a / b,
                    BinaryExpression.ExpressionType.Modulo => (a, b) => a % b,
                    BinaryExpression.ExpressionType.Power => (a, b) => DecimalMath.Pow(a, b),
                    BinaryExpression.ExpressionType.And => (a, b) => a != 0 && b != 0 ? 1 : 0,
                    BinaryExpression.ExpressionType.NAnd => (a, b) => a != 0 && b != 0 ? 0 : 1,
                    BinaryExpression.ExpressionType.Or => (a, b) => a != 0 || b != 0 ? 1 : 0,
                    BinaryExpression.ExpressionType.NOr => (a, b) => a != 0 || b != 0 ? 0 : 1,
                    BinaryExpression.ExpressionType.Xor => (a, b) => a != 0 ^ b != 0 ? 1 : 0,
                    BinaryExpression.ExpressionType.XNor => (a, b) => a != 0 ^ b != 0 ? 0 : 1,
                    BinaryExpression.ExpressionType.Equals => (a, b) => a == b ? 1 : 0,
                    BinaryExpression.ExpressionType.Inequals => (a, b) => a == b ? 0 : 1,
                    BinaryExpression.ExpressionType.Less => (a, b) => a < b ? 1 : 0,
                    BinaryExpression.ExpressionType.Greater => (a, b) => a > b ? 1 : 0,
                    BinaryExpression.ExpressionType.LessEq => (a, b) => a <= b ? 1 : 0,
                    BinaryExpression.ExpressionType.GreaterEq => (a, b) => a >= b ? 1 : 0,
                    _ => throw new InvalidOperationException("Attempting to aggregate unknown operation")
                });
                foreach (var e in arr)
                    list.Remove(e);
                list.Add(new LiteralExpression(sum).WithToken(expr.Token));
            }
            if (list.Count < 2) return list.First();
            return new BinaryExpression(expr.Type, list).WithToken(expr.Token);
        }
        /// <inheritdoc/>
        public override MathExpression ApplyTo(UnaryExpression expr, IOptimizationContext<object?> ctx, out bool transformResult)
        {
            transformResult = true;
            var arg = ApplyTo(expr.Argument, ctx);
            if (arg is LiteralExpression l)
            {
                switch (expr.Type)
                {
                    case UnaryExpression.ExpressionType.Negate:
                        return new LiteralExpression(-l.Value).WithToken(expr.Token);
                    case UnaryExpression.ExpressionType.Not:
                        return new LiteralExpression(l.Value != 0 ? 0 : 1).WithToken(expr.Token);
                    case UnaryExpression.ExpressionType.Factorial:
                        try
                        {
                            return new LiteralExpression(DecimalMath.Factorial(l.Value)).WithToken(expr.Token);
                        }
                        catch (InvalidOperationException)
                        {
                            // ignore it
                            break;
                        }
                        catch (OverflowException)
                        {
                            // ignore it
                            break;
                        }
                }
            }

            return new UnaryExpression(expr.Type, arg).WithToken(expr.Token);
        }
    }
}
