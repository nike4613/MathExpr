using MathExpr.Syntax;
using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MathExpr.Compiler.Compilation.Builtins
{
    /// <summary>
    /// An <see cref="IBuiltinFunction{TSettings}"/> representing a trigonometric function.
    /// </summary>
    /// <remarks>
    /// This is a specialization that only accepts a single argument of type <see cref="double"/>, 
    /// and returns a <see cref="double"/>.
    /// </remarks>
    public abstract class BuiltinTrigFunction : IBuiltinFunction<object?>
    {
        /// <inheritdoc/>
        public abstract string Name { get; }
        /// <summary>
        /// The actual method that implements this trigonometric function.
        /// </summary>
        protected abstract MethodInfo Method { get; }
        /// <inheritdoc/>
        public bool TryCompile(IReadOnlyList<MathExpression> arguments, ICompilationContext<object?> context, ITypeHintHandler typeHintHandler, out Expression expr)
        {
            // TODO: add logging to contexts
            if (arguments.Count != 1)
            {
                expr = default!;
                return false;
            }

            expr = Expression.Call(Method, 
                CompilerHelpers.ConvertToType(
                    typeHintHandler.TransformWithHint(arguments.First(), typeof(double), context), 
                    typeof(double)));
            return true;
        }
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>sin(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Sin(double)"/>.
    /// </remarks>
    public class BuiltinFunctionSin : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "sin";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Sin(d))!;
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>asin(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Asin(double)"/>.
    /// </remarks>
    public class BuiltinFunctionAsin : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "asin";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Asin(d))!;
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>cos(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Cos(double)"/>.
    /// </remarks>
    public class BuiltinFunctionCos : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "cos";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Cos(d))!;
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>acos(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Acos(double)"/>.
    /// </remarks>
    public class BuiltinFunctionAcos : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "acos";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Acos(d))!;
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>tan(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Tan(double)"/>.
    /// </remarks>
    public class BuiltinFunctionTan : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "tan";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Tan(d))!;
    }

    /// <summary>
    /// An implementation of <see cref="BuiltinTrigFunction"/> for the <c>atan(x)</c> function.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="Math.Atan(double)"/>.
    /// </remarks>
    public class BuiltinFunctionAtan : BuiltinTrigFunction
    {
        /// <summary>
        /// The name of this builtin as a <see langword="const"/> <see cref="string"/>.
        /// </summary>
        public const string ConstName = "atan";
        /// <inheritdoc/>
        public override string Name => ConstName;
        /// <inheritdoc/>
        protected override MethodInfo Method { get; } = Helpers.GetMethod<Action<double>>(d => Math.Atan(d))!;
    }
}
