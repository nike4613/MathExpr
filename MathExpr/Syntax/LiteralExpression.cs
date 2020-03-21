using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing a literal value.
    /// </summary>
    public class LiteralExpression : MathExpression
    {
        /// <summary>
        /// The literal value entered into the source.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// The size of this expression. This is always 0, since no operation is performed.
        /// </summary>
        public override int Size => 0; // because we don't perform any operation
        
        /// <summary>
        /// Constructs a new <see cref="LiteralExpression"/> with the specified value.
        /// </summary>
        /// <param name="value">the value of the expression</param>
        public LiteralExpression(decimal value)
            => Value = value;

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is LiteralExpression l && l.Value == Value;

        /// <inheritdoc/>
        public override string ToString()
            => Value.ToString();

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = -159790080;
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
    }
}
