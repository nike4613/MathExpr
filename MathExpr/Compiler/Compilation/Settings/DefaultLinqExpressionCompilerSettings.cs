using MathExpr.Compiler.Compilation.Builtins;
using MathExpr.Compiler.Optimization.Settings;
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
    /// A default implementation of <see cref="ICompileToLinqExpressionSettings{TSettings}"/>
    /// </summary>
    public class DefaultLinqExpressionCompilerSettings : 
        ICompileToLinqExpressionSettings<DefaultLinqExpressionCompilerSettings>,
        IWritableCompileToLinqExpressionSettings,
        IBuiltinFunctionCompilerSettings<DefaultLinqExpressionCompilerSettings>,
        IBuiltinFunctionWritableCompilerSettings<DefaultLinqExpressionCompilerSettings>,
        IDomainRestrictionSettings
    {
        /// <inheritdoc/>
        public Type ExpectReturn { get; set; } = typeof(decimal);

        /// <inheritdoc/>
        public IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; } = new Dictionary<VariableExpression, ParameterExpression>();

        /// <inheritdoc/>
        public IReadOnlyCollection<IBuiltinFunction<DefaultLinqExpressionCompilerSettings>> BuiltinFunctions => builtinFunctions;
        /// <inheritdoc/>
        public ICollection<IBuiltinFunction<DefaultLinqExpressionCompilerSettings>> WritableBuiltinFunctions => builtinFunctions;

        private readonly List<IBuiltinFunction<DefaultLinqExpressionCompilerSettings>> builtinFunctions = new List<IBuiltinFunction<DefaultLinqExpressionCompilerSettings>>();

        /// <summary>
        /// Initializes a default configuration.
        /// </summary>
        public DefaultLinqExpressionCompilerSettings()
        {
            this.AddBuiltin().OfType<BuiltinFunctionIf>();
            this.AddBuiltin().OfType<BuiltinFunctionExp>();
            this.AddBuiltin().OfType<BuiltinFunctionLn>();
            this.AddBuiltin().OfType<BuiltinFunctionSin>();
            this.AddBuiltin().OfType<BuiltinFunctionAsin>();
            this.AddBuiltin().OfType<BuiltinFunctionCos>();
            this.AddBuiltin().OfType<BuiltinFunctionAcos>();
            this.AddBuiltin().OfType<BuiltinFunctionTan>();
            this.AddBuiltin().OfType<BuiltinFunctionAtan>();
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
