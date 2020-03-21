using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing an operation with only one argument.
    /// </summary>
    public sealed class UnaryExpression : MathExpression
    {
        /// <summary>
        /// The type of the <see cref="UnaryExpression"/>.
        /// </summary>
        [SuppressMessage("Documentation", "CS1591", Justification = "The names are self-explanatory.")]
        public enum ExpressionType
        {
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
            Negate, Not, Factorial
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// The type of this expression.
        /// </summary>
        public ExpressionType Type { get; }
        /// <summary>
        /// The single argument to this expression.
        /// </summary>
        public MathExpression Argument { get; }

        /// <summary>
        /// The size of this expression. This is always the size of the argument plus one.
        /// </summary>
        public override int Size => Argument.Size + 1;

        /// <summary>
        /// Constructs a new <see cref="UnaryExpression"/> of the specified type, with the given argument.
        /// </summary>
        /// <param name="type">the type of the expression</param>
        /// <param name="arg">the argument of the expression</param>
        public UnaryExpression(ExpressionType type, MathExpression arg)
        {
            Type = type;
            Argument = arg;
        }

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is UnaryExpression e 
            && Type == e.Type 
            && Equals(Argument, e.Argument);

        /// <inheritdoc/>
        public override string ToString()
            => $"({Type} {Argument})";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = -850124847;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Argument);
            return hashCode;
        }
    }
}
