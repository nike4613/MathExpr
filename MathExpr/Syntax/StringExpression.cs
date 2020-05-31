using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing a literal string.
    /// </summary>
    public class StringExpression : MathExpression
    {
        /// <summary>
        /// The literal value of the string.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public override int Size => 0;

        /// <summary>
        /// Creates a <see cref="StringExpression"/> with the given string as its value.
        /// </summary>
        /// <param name="value">the value of the string</param>
        public StringExpression(string value) => Value = value;

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is StringExpression str && Value == str.Value;

        /// <inheritdoc/>
        public override int GetHashCode()
            => Value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
            => $"\"{Value.Replace("\"", "\\\"")}\""; // TODO: handle more escapes properly
    }
}
