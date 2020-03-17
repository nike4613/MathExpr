using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MathExpr.Compiler.Compilation.Passes
{

    public class BasicCompileToLinqExpressionPass : CompilationTransformPass<ICompileToLinqExpressionSettings>
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

        public override Expression ApplyTo(Syntax.BinaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(Syntax.UnaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var arg = ApplyTo(expr.Argument, ctx);
            return expr.Type switch
            {
                Syntax.UnaryExpression.ExpressionType.Negate => Expression.Negate(arg),
                Syntax.UnaryExpression.ExpressionType.Not => // we use a value of 0 to represent false, and nonzero for true
                    Expression.Condition(
                        Expression.NotEqual(arg, ConstantOfType(arg.Type, 0)),
                        ConstantOfType(arg.Type, 0),
                        ConstantOfType(arg.Type, 1)),
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

        public override Expression ApplyTo(FunctionExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            if (expr.IsPrime)
                throw new InvalidOperationException("Default compiler does not support un-inlined user functions");

            var name = expr.Name;
            var args = expr.Arguments;

            if (ctx.Settings.BuiltinFunctions.TryGetValue((name, args.Count), out var builtin))
            {
                var hint = GetTypeHint(ctx);
                if (!builtin.TryCompile(args, ctx, SetTypeHint, out var outexpr))
                    throw new InvalidOperationException($"Function '{name}' coult not compile with the given arguments");
                SetTypeHint(ctx, hint);
                return outexpr;
            }
            else
                throw new InvalidOperationException($"Builtin function named '{name}' with {args.Count} arguments does not exist");
        }

        private Expression ConstantOfType(Type type, object? val)
        {
            try
            {
                return Expression.Constant(Convert.ChangeType(val, type), type);
            }
            catch (InvalidCastException)
            {
                // fallback to runtime conversion if possible
                return Expression.Convert(Expression.Constant(val), type);
            }
        }

        public override Expression ApplyTo(LiteralExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var hint = GetTypeHint(ctx);
            if (hint != null)
                try
                {
                    return ConstantOfType(hint, expr.Value);
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
