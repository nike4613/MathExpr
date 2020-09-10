using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading;

namespace MathExpr.Syntax
{
    /// <summary>
    /// A <see cref="MathExpression"/> representing an operator with 2 arguments.
    /// </summary>
    public sealed class BinaryExpression : MathExpression
    {
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class BooleanAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Field)]
        internal sealed class ComparisonAttribute : Attribute { }

        /// <summary>
        /// The type of a <see cref="BinaryExpression"/>.
        /// </summary>
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

            _Count
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
            if (type < ExpressionType.Add || type >= ExpressionType._Count)
                throw new ArgumentException("Invalid ExpressionType value", nameof(type));
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
            if (type < ExpressionType.Add || type >= ExpressionType._Count)
                throw new ArgumentException("Invalid ExpressionType value", nameof(type));
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

    /// <summary>
    /// A type that provides extension methods for <see cref="BinaryExpression.ExpressionType"/>.
    /// </summary>
    public static class BinaryExpressionTypeExtensions
    {
        private const int BitsPerFlag = 2;
        private const int FlagsPerItem = sizeof(int) * (8 / BitsPerFlag); // 2 bit flags
        private static readonly int[] typeflags = new int[((int)BinaryExpression.ExpressionType._Count + FlagsPerItem - 1) / FlagsPerItem];

        private const int Flags = 0b11;
        private const int FlagUnset = 0b00;
        private const int FlagNothing = 0b01;
        private const int FlagBoolean = 0b10;
        private const int FlagComparison = 0b11;

        /// <summary>
        /// Gets whether or not <paramref name="type"/> represents a boolean operation.
        /// </summary>
        /// <param name="type">The <see cref="BinaryExpression.ExpressionType"/> to check.</param>
        /// <returns><see langword="true"/> if <paramref name="type"/> is a boolean operation, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBooleanType(this BinaryExpression.ExpressionType type)
        {
            Validate(type);

            var idx = (int)type / FlagsPerItem;
            var offs = (int)type % FlagsPerItem;

            var flags = (typeflags[idx] >> (offs * BitsPerFlag)) & Flags;

            if (flags == FlagUnset)
            {
                flags = InitFlagsFor(type);
            }

            return flags == FlagBoolean;
        }

        /// <summary>
        /// Gets whether or not <paramref name="type"/> represents a comparison operation.
        /// </summary>
        /// <param name="type">The <see cref="BinaryExpression.ExpressionType"/> to check.</param>
        /// <returns><see langword="true"/> if <paramref name="type"/> is a comparison operation, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsComparisonType(this BinaryExpression.ExpressionType type)
        {
            Validate(type);

            var idx = (int)type / FlagsPerItem;
            var offs = (int)type % FlagsPerItem;

            var flags = (typeflags[idx] >> (offs * BitsPerFlag)) & Flags;

            if (flags == FlagUnset)
            {
                flags = InitFlagsFor(type);
            }

            return flags == FlagComparison;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Validate(BinaryExpression.ExpressionType type)
        {
            if (type < BinaryExpression.ExpressionType.Add || type >= BinaryExpression.ExpressionType._Count)
                throw new ArgumentException("Invalid ExpressionType value", nameof(type));
        }

        private static int InitFlagsFor(BinaryExpression.ExpressionType type)
        {
            var field = typeof(BinaryExpression.ExpressionType).GetField(type.ToString(), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new InvalidOperationException("Could not get field for expression type");

            int flags = FlagNothing;
            if (field.GetCustomAttribute<BinaryExpression.BooleanAttribute>() != null)
                flags = FlagBoolean;
            else if (field.GetCustomAttribute<BinaryExpression.ComparisonAttribute>() != null)
                flags = FlagComparison;

            var idx = (int)type / FlagsPerItem;
            var offs = (int)type % FlagsPerItem;

            var bitoffs = offs * BitsPerFlag;
            int read2;
            var read = typeflags[idx];
            do
            {
                read2 = read;
                read &= ~(Flags << bitoffs);
                read |= flags << bitoffs;
                read = Interlocked.CompareExchange(ref typeflags[idx], read, read2);
            }
            while (read != read2);

            return flags;
        }
    }

}
