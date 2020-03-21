using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing a variable.
    /// </summary>
    public sealed class VariableExpression : MathExpression
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The size of the expression. 
        /// </summary>
        public override int Size => 1; // because it is one operation to load

        /// <summary>
        /// Creates a new <see cref="VariableExpression"/> with the specified variable name.
        /// </summary>
        /// <param name="name">the name of the variable</param>
        public VariableExpression(string name)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is VariableExpression v && v.Name == Name;

        /// <inheritdoc/>
        public override string ToString()
            => $"'{Name}'";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 890389916;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
