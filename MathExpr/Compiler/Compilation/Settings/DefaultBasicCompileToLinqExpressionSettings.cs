using MathExpr.Compiler.Compilation.Builtins;
using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    /// <summary>
    /// A default implementation of <see cref="ICompileToLinqExpressionSettings"/>
    /// </summary>
    public class DefaultBasicCompileToLinqExpressionSettings : ICompileToLinqExpressionSettings
    {
        /// <inheritdoc/>
        public Type ExpectReturn { get; set; } = typeof(decimal);

        /// <inheritdoc/>
        public IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; } = new Dictionary<VariableExpression, ParameterExpression>();

        /// <inheritdoc/>
        public IDictionary<string, IList<IBuiltinFunction<ICompileToLinqExpressionSettings>>> BuiltinFunctions { get; }
            = new Dictionary<string, IList<IBuiltinFunction<ICompileToLinqExpressionSettings>>>();
        
        /// <summary>
        /// Initializes a default configuration.
        /// </summary>
        public DefaultBasicCompileToLinqExpressionSettings()
        {
            this.AddBuiltin().OfType<BuiltinFunctionIf>();
        }

        #region Domain Restrictions
        /// <inheritdoc/>
        public bool IgnoreDomainRestrictions { get; set; } = false;

        /// <inheritdoc/>
        public bool AllowDomainChangingOptimizations { get; set; } = true;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        #region Exponent Compilers
        /// <inheritdoc/>
        public IList<ISpecialBinaryOperationCompiler> PowerCompilers { get; set; }
            = new List<ISpecialBinaryOperationCompiler>
            {
                new FloaingPointPowerCompiler(),
                new OtherNumericPowerCompiler(),
            };
        #endregion
    }
}
