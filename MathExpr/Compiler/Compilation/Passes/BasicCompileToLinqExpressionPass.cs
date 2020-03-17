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

            // TODO: use subexpr

            return subexpr;
        }

        public override Expression ApplyTo(Syntax.BinaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(Syntax.UnaryExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(Syntax.MemberExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(VariableExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(FunctionExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }

        public override Expression ApplyTo(LiteralExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            var hint = GetTypeHint(ctx);
            if (hint != null)
                return Expression.Constant(Convert.ChangeType(expr.Value, hint), hint);
            else
                return Expression.Constant(expr.Value);
        }

        public override Expression ApplyTo(CustomDefinitionExpression expr, ICompilationTransformContext<ICompileToLinqExpressionSettings> ctx)
        {
            throw new NotImplementedException();
        }
    }
}
