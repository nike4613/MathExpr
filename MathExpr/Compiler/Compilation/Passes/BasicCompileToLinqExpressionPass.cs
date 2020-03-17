using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MathExpr.Compiler.Compilation.Passes
{

    public class BasicCompileToLinqExpressionPass : CompilationTransformPass<ICompileToLinqExpressionSettings>, ITypeHintHandler
    {
        private sealed class HasParentWithRoot { }
        private sealed class TypeHint { }

        private bool IsRootExpression(ITransformContext ctx)
            => !ctx.Data<bool>().GetOrCreateIn<HasParentWithRoot>(false);
        private void SetRootExpression(ITransformContext ctx)
            => ctx.Data<bool>().SetIn<HasParentWithRoot>(true);

        private Type? GetTypeHint(ITransformContext ctx)
            => ctx.Data<Type?>().GetOrDefaultIn<TypeHint>();
        private void SetTypeHint(ITransformContext ctx, Type? hint)
            => ctx.Data<Type?>().SetIn<TypeHint>(hint);

        public override Expression ApplyTo(MathExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            if (!IsRootExpression(ctx))
                return base.ApplyTo(expr, ctx);

            SetRootExpression(ctx);

            // TODO: set up lambda stuff

            SetTypeHint(ctx, ctx.Settings.ExpectReturn);
            var subexpr = base.ApplyTo(expr, ctx);
            if (subexpr.Type != ctx.Settings.ExpectReturn)
                subexpr = Expression.Convert(subexpr, ctx.Settings.ExpectReturn);

            // TODO: use subexpr

            return subexpr;
        }

        private Func<Expression, Expression, Expression> SpecialBinaryAggregator(IEnumerable<ISpecialBinaryOperationCompiler> set)
            => (a, b) =>
            {
                Expression? result = null;
                foreach (var comp in set)
                    if (comp.TryCompile(a, b, out result)) break;
                if (result == null) throw new InvalidOperationException("No exponent implementation for types");
                return result!;
            };

        public override Expression ApplyTo(Syntax.BinaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var args = expr.Arguments.Select(m => ApplyTo(m, ctx));
            var boolResType = GetTypeHint(ctx) ?? args.First().Type;
            return expr.Type switch
            {
                Syntax.BinaryExpression.ExpressionType.Add
                    => args.Aggregate(Expression.Add),
                Syntax.BinaryExpression.ExpressionType.Subtract
                    => args.Aggregate(Expression.Subtract),
                Syntax.BinaryExpression.ExpressionType.Multiply
                    => args.Aggregate(Expression.Multiply),
                Syntax.BinaryExpression.ExpressionType.Divide
                    => args.Aggregate(Expression.Divide),
                Syntax.BinaryExpression.ExpressionType.Modulo
                    => args.Aggregate(Expression.Modulo),
                Syntax.BinaryExpression.ExpressionType.Power
                    => args.Aggregate(SpecialBinaryAggregator(ctx.Settings.PowerCompilers)),

                Syntax.BinaryExpression.ExpressionType.And
                    => CompilerHelpers.BoolToNumBool(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.AndAlso), boolResType),
                Syntax.BinaryExpression.ExpressionType.NAnd
                    => CompilerHelpers.BoolToNumBool(Expression.IsFalse(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.AndAlso)), boolResType),
                Syntax.BinaryExpression.ExpressionType.Or
                    => CompilerHelpers.BoolToNumBool(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.OrElse), boolResType),
                Syntax.BinaryExpression.ExpressionType.NOr
                    => CompilerHelpers.BoolToNumBool(Expression.IsFalse(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.OrElse)), boolResType),
                Syntax.BinaryExpression.ExpressionType.Xor
                    => CompilerHelpers.BoolToNumBool(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.NotEqual), boolResType),
                Syntax.BinaryExpression.ExpressionType.XNor
                    => CompilerHelpers.BoolToNumBool(args.Select(CompilerHelpers.AsBoolean).Aggregate(Expression.Equal), boolResType),

                Syntax.BinaryExpression.ExpressionType.Equals
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.Equal), boolResType),
                Syntax.BinaryExpression.ExpressionType.Inequals
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.NotEqual), boolResType),
                Syntax.BinaryExpression.ExpressionType.Less
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.LessThan), boolResType),
                Syntax.BinaryExpression.ExpressionType.LessEq
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.LessThanOrEqual), boolResType),
                Syntax.BinaryExpression.ExpressionType.Greater
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.GreaterThan), boolResType),
                Syntax.BinaryExpression.ExpressionType.GreaterEq
                    => CompilerHelpers.BoolToNumBool(args.Aggregate(Expression.GreaterThanOrEqual), boolResType),

                _ => throw new InvalidOperationException("Invalid type of binary expression"),
            };
        }

        public override Expression ApplyTo(Syntax.UnaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var arg = ApplyTo(expr.Argument, ctx);
            return expr.Type switch
            {
                Syntax.UnaryExpression.ExpressionType.Negate => Expression.Negate(arg),
                Syntax.UnaryExpression.ExpressionType.Not => // we use a value of 0 to represent false, and nonzero for true
                    CompilerHelpers.CoerceNumBoolean(arg, true),
                Syntax.UnaryExpression.ExpressionType.Factorial => 
                    ctx.Settings.TypedFactorialCompilers.TryGetValue(arg.Type, out var func)
                    ? func(arg)
                    : throw new InvalidOperationException("Applying factorial to type with no compiler"),
                _ => throw new InvalidOperationException("Invalid expression type")
            };
        }

        public override Expression ApplyTo(Syntax.MemberExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var arg = ApplyTo(expr.Target, ctx);
            var name = expr.MemberName;

            var type = arg.Type;
            if (type.GetProperty(name) is PropertyInfo prop)
                return Expression.MakeMemberAccess(arg, prop);
            if (type.GetField(name) is FieldInfo field)
                return Expression.MakeMemberAccess(arg, field);

            throw new MemberAccessException($"Expression of type {type} does not have member '{name}'");
        }

        public override Expression ApplyTo(VariableExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            if (ctx.Settings.ParameterMap.TryGetValue(expr, out var param))
                return param;
            else
                throw new InvalidOperationException($"Variable '{expr.Name}' does not have an associated ParameterExpression");
        }

        Expression ITypeHintHandler.TransformWithHint<TSettings>(MathExpression expr, Type hint, ICompilationTransformContext<TSettings> ctx)
        {
            var savedHint = GetTypeHint(ctx);
            SetTypeHint(ctx, hint);
            var outExpr = ctx.Transform(expr);
            SetTypeHint(ctx, savedHint);
            return outExpr;
        }

        public override Expression ApplyTo(FunctionExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            if (expr.IsPrime)
                throw new InvalidOperationException("Default compiler does not support un-inlined user functions");

            var name = expr.Name;
            var args = expr.Arguments;

            if (ctx.Settings.BuiltinFunctions.TryGetValue((name, args.Count), out var builtin))
            {
                if (!builtin.TryCompile(args, ctx, this, out var outexpr))
                    throw new InvalidOperationException($"Function '{name}' coult not compile with the given arguments");
                return outexpr;
            }
            else
                throw new InvalidOperationException($"Builtin function named '{name}' with {args.Count} arguments does not exist");
        }

        public override Expression ApplyTo(LiteralExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var hint = GetTypeHint(ctx);
            if (hint != null)
                try
                {
                    return CompilerHelpers.ConstantOfType(hint, expr.Value);
                }
                catch (Exception)
                {
                    // ignore, fall through to regular constant expr
                }

            return Expression.Constant(expr.Value);
        }

        public override Expression ApplyTo(CustomDefinitionExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new InvalidOperationException("Default compiler does not support un-inlined user functions");
        }

    }
}
