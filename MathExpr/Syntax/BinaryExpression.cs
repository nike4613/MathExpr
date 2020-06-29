using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace MathExpr.Syntax
{
    /// <summary>
    /// A <see cref="MathExpression"/> representing an operator with 2 arguments.
    /// </summary>
    public sealed class BinaryExpression : MathExpression
    {
        [AttributeUsage(AttributeTargets.Field)]
        private sealed class BooleanAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Field)]
        private sealed class ComparisonAttribute : Attribute { }

        /// <summary>
        /// The type of a <see cref="BinaryExpression"/>.
        /// </summary>
        [SuppressMessage("Documentation", "CS1591", Justification = "The names are self-explanatory.")]
        public enum ExpressionType
        {
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
            Add, Subtract, Multiply, Divide, Modulo, Power,
            [Boolean] And, [Boolean] NAnd, 
            [Boolean] Or, [Boolean] NOr, 
            [Boolean] Xor, [Boolean] XNor,
            [Comparison] Equals, [Comparison] Inequals, 
            [Comparison] Less, [Comparison] LessEq,
            [Comparison] Greater, [Comparison] GreaterEq,
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// The type of this expression.
        /// </summary>
        public ExpressionType Type { get; }
        /// <summary>
        /// The left argument of the operation.
        /// </summary>
        public MathExpression Left => Arguments[0];
        /// <summary>
        /// The right argument of the expression.
        /// </summary>
        public MathExpression Right => Arguments[1];

        /// <summary>
        /// All of the arguments for this expression.
        /// </summary>
        public IReadOnlyList<MathExpression> Arguments { get; }

        /// <inheritdoc/>
        /// <remarks>
        /// The size of a <see cref="BinaryExpression"/> is the sum of the sizes of its arguments,
        /// plus the number of arguments minus 1.
        /// </remarks>
        public override int Size => Arguments.Sum(a => a.Size) + (Arguments.Count - 1);

        /// <summary>
        /// Constructs a new <see cref="BinaryExpression"/> consisting of the specified arguments and type.
        /// </summary>
        /// <param name="left">the left argument</param>
        /// <param name="right">the right argument</param>
        /// <param name="type">the type of the expression</param>
        public BinaryExpression(MathExpression left, MathExpression right, ExpressionType type)
        {
            Type = type;
            Arguments = new List<MathExpression> { left, right };
        }

        /// <summary>
        /// Constructs a new <see cref="BinaryExpression"/> consisting of the specified arguments and type.
        /// </summary>
        /// <param name="type">the type fo the expression</param>
        /// <param name="args">the operation arguments</param>
        public BinaryExpression(ExpressionType type, IReadOnlyList<MathExpression> args)
        {
            if (args.Count < 2)
                throw new ArgumentException("A BinaryExpression must have at least 2 arguments", nameof(args));
            if (args.Count > 2)
                switch (type)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Multiply:
                    case ExpressionType.And:
                    case ExpressionType.Or:
                        break;
                    default:
                        throw new ArgumentException("Can only have more than 2 arguments when the type is commutative");
                }
            Type = type;
            Arguments = args;
        }

        /// <summary>
        /// Compares this expression to the parameter for equality.
        /// </summary>
        /// <param name="other">the expression to compare to</param>
        /// <returns><see langword="true"/> if the two are equal, <see langword="false"/> otherwise</returns>
        public override bool Equals(MathExpression other)
            => other is BinaryExpression e
            && Arguments.Count == e.Arguments.Count
            && Arguments.Zip(e.Arguments, (a, b) => Equals(a, b)).All(b => b);
        // TODO: make Equals not care about order for commutative operators (for common subexpression elimination)

        /// <summary>
        /// Returns a string representation of the operation.
        /// </summary>
        /// <returns>a string representation of the operation</returns>
        public override string ToString()
            => $"({string.Join($" {Type} ", Arguments)})";

        /// <summary>
        /// Gets a hashcode that represents this expression.
        /// </summary>
        /// <returns>a hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 1099731784;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<MathExpression>>.Default.GetHashCode(Arguments);
            return hashCode;
        }

    }
}
