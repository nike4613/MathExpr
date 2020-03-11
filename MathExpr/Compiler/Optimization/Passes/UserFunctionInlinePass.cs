using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Compiler.Optimization.Passes
{
    public class UserFunctionInlinePass : OptimizationPass<IFunctionInlineSettings>
    {
        // name -> function info
        private Dictionary<string, (CustomDefinitionExpression func, bool hasNotInlinedUses)> GetDefinedFunctions(IOptimizationContext<IFunctionInlineSettings> ctx)
            => ctx.Data<Dictionary<string, (CustomDefinitionExpression func, bool hasNotInlinedUses)>>().GetOrCreateIn(ctx.Settings);

        public override MathExpression ApplyTo(CustomDefinitionExpression expr, IOptimizationContext<IFunctionInlineSettings> ctx, out bool transformResult)
        {
            if (ctx.Settings.ShouldInline && expr.DefinitionSize <= ctx.Settings.DoNotInlineAfterSize)
            {
                var definedFunctions = GetDefinedFunctions(ctx);

                // TODO: track overloads with different arg count
                definedFunctions.Add(expr.FunctionName, (expr, false));

                var value = ApplyTo(expr.Value, ctx);
                transformResult = false; // because the resulting value has already been fully transformed
                if (definedFunctions[expr.FunctionName].hasNotInlinedUses)
                    return new CustomDefinitionExpression(expr.FunctionName, expr.ArgumentList, expr.Definition, value);
                else
                    return value;
            }

            return base.ApplyTo(expr, ctx, out transformResult);
        }

        private Dictionary<VariableExpression, MathExpression> GetVariableSubstitutions(IOptimizationContext<IFunctionInlineSettings> ctx)
            => ctx.Data<Dictionary<VariableExpression, MathExpression>>().GetOrCreateIn(this);

        public override MathExpression ApplyTo(FunctionExpression expr, IOptimizationContext<IFunctionInlineSettings> ctx, out bool transformResult)
        {
            if (expr.IsPrime)
            {
                var definedFunctions = GetDefinedFunctions(ctx);

                if (definedFunctions.TryGetValue(expr.Name, out var tup) && expr.Arguments.Count == tup.func.ArgumentList.Count)
                {
                    var variableSubs = GetVariableSubstitutions(ctx);
                    if (variableSubs.Count == 0)
                    { // only when we're not currently substituting
                        foreach (var (replace, with) in tup.func.ArgumentList.Zip(expr.Arguments, (a, b) => (a, b)))
                            variableSubs.Add(replace, with);

                        var value = ApplyTo(tup.func.Definition, ctx);

                        variableSubs.Clear();

                        transformResult = false; // because we have applied all transformations by definition with the following call
                        // this could very easily create dangerously deep callstacks
                        return ApplyTo(value, ctx); // handle nested calls
                    }
                }
            }

            return base.ApplyTo(expr, ctx, out transformResult);
        }

        public override MathExpression ApplyTo(VariableExpression expr, IOptimizationContext<IFunctionInlineSettings> ctx, out bool transformResult)
        {
            var variableSubs = GetVariableSubstitutions(ctx);
            transformResult = true;
            if (variableSubs.TryGetValue(expr, out var replaceWith))
                return replaceWith;

            return base.ApplyTo(expr, ctx, out transformResult);
        }
    }
}
