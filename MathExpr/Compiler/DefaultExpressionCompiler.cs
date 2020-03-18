using MathExpr.Compiler.Compilation;
using MathExpr.Compiler.Compilation.Passes;
using MathExpr.Compiler.Compilation.Settings;
using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler
{
    public class DefaultExpressionCompiler : ExpressionCompiler<DefaultOptimizationSettings, DefaultBasicCompileToLinqExpressionSettings>
    {
        public DefaultExpressionCompiler() : this(new DefaultOptimizationSettings(), new DefaultBasicCompileToLinqExpressionSettings())
        {
        }

        public DefaultExpressionCompiler(DefaultOptimizationSettings optimizer, DefaultBasicCompileToLinqExpressionSettings compilerSettings) 
            : base(optimizer, compilerSettings, new BasicCompileToLinqExpressionPass())
        {
        }

        public new Expression CompileToExpression(MathExpression expr, bool optimize = true)
        {
            if (optimize)
            {
                expr = Optimize(expr);
                // TODO: make this not actually necessary somehow
                foreach (var restrict in OptimizerSettings.DomainRestrictions)
                    CompilerSettings.DomainRestrictions.Add(restrict);
            }
            return base.CompileToExpression(expr, false);
        }

        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, IEnumerable<string> argumentNames)
            where TDelegate : Delegate
        {
            var parameters = DelegateInformation<TDelegate>.ParamTypes
                .Zip(argumentNames, (type, name) => (type, name))
                .Select(t => (t.name, param: Expression.Parameter(t.type, t.name)))
                .ToList();

            if (parameters.Count != DelegateInformation<TDelegate>.ParamTypes.Length)
                throw new ArgumentException("Incorrect number of argument names", nameof(argumentNames));

            CompilerSettings.ExpectReturn = DelegateInformation<TDelegate>.ReturnType;
            CompilerSettings.ParameterMap.Clear();
            foreach (var (name, param) in parameters)
                CompilerSettings.ParameterMap.Add(new VariableExpression(name), param);

            return compile(
                Expression.Lambda<TDelegate>(
                    CompileToExpression(expr, optimize),
                    parameters.Select(p => p.param)));
        }
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, Func<Expression<TDelegate>, TDelegate> compile, params string[] argumentNames)
            where TDelegate : Delegate
            => Compile(expr, optimize, compile, (IEnumerable<string>)argumentNames);
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, IEnumerable<string> argumentNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, e => e.Compile(), argumentNames);
        public TDelegate Compile<TDelegate>(MathExpression expr, bool optimize, params string[] argumentNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, optimize, (IEnumerable<string>)argumentNames);
        public TDelegate Compile<TDelegate>(MathExpression expr, IEnumerable<string> argumentNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, true, e => e.Compile(), argumentNames);
        public TDelegate Compile<TDelegate>(MathExpression expr, params string[] argumentNames)
            where TDelegate : Delegate
            => Compile<TDelegate>(expr, (IEnumerable<string>)argumentNames);
    }
}
