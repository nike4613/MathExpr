using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    public class DefaultBasicCompileToLinqExpressionSettings : ICompileToLinqExpressionSettings
    {
        public Type ExpectReturn { get; set; } = typeof(decimal);

        public IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; } = new Dictionary<VariableExpression, ParameterExpression>();

        public IDictionary<(string name, int argcount), IBuiltinFunction<ICompileToLinqExpressionSettings>> BuiltinFunctions { get; }
            = new Dictionary<(string name, int argcount), IBuiltinFunction<ICompileToLinqExpressionSettings>>();

        #region Domain Restrictions
        public bool IgnoreDomainRestrictions { get; set; } = false;

        public bool AllowDomainChangingOptimizations { get; set; } = true;

        public IList<MathExpression> DomainRestrictions { get; } = new List<MathExpression>();
        #endregion

        #region Factorial Compilers
        private static readonly MethodInfo IntFactorialMethod = Helpers.GetMethod<Func<ulong, ulong>>(a => Helpers.IntegerFactorial(a))!;
        private static readonly MethodInfo DecimalFactorialMethod = Helpers.GetMethod<Func<decimal, decimal>>(a => DecimalMath.Factorial(a))!;

        // the resulting expression returns a long 
        private static Expression TypedIntegerFactorialCompiler<T>(Expression arg)
            => Expression.ConvertChecked(Expression.Call(IntFactorialMethod, Expression.ConvertChecked(arg, typeof(ulong))), typeof(long));
        private static Expression TypedFloatingFactorialCompiler<T>(Expression arg)
            => Expression.ConvertChecked(Expression.Call(DecimalFactorialMethod, Expression.ConvertChecked(arg, typeof(decimal))), typeof(T));

        public IDictionary<Type, TypedFactorialCompiler> TypedFactorialCompilers { get; }
            = new Dictionary<Type, TypedFactorialCompiler>
            {
                { typeof(ulong), e => Expression.Call(IntFactorialMethod, e) },
                { typeof(long), TypedIntegerFactorialCompiler<long> },
                { typeof(uint), TypedIntegerFactorialCompiler<uint> },
                { typeof(int), TypedIntegerFactorialCompiler<int> },
                { typeof(ushort), TypedIntegerFactorialCompiler<ushort> },
                { typeof(short), TypedIntegerFactorialCompiler<short> },
                { typeof(byte), TypedIntegerFactorialCompiler<byte> },
                { typeof(sbyte), TypedIntegerFactorialCompiler<sbyte> },

                { typeof(decimal), e => Expression.Call(DecimalFactorialMethod, e) },
                { typeof(double), TypedFloatingFactorialCompiler<double> },
                { typeof(float), TypedFloatingFactorialCompiler<float> },
            };
        #endregion
    }
}
