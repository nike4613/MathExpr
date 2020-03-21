using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// The base class for all expression types. Represents an evaluatable mathematical expression.
    /// </summary>
    public abstract class MathExpression
    {
        internal MathExpression() { }

        /// <summary>
        /// Parses an exression string into an expression tree.
        /// </summary>
        /// <param name="expr">the string to parse</param>
        /// <returns>the expression tree</returns>
        /// <exception cref="SyntaxException">when there is a syntax error</exception>
        public static MathExpression Parse(string expr)
            => ExpressionParser.ParseRoot(expr);

        /// <summary>
        /// The size of the expression. This is roughly the number of operations it contains
        /// </summary>
        public abstract int Size { get; }
        /// <summary>
        /// Compares this expression to the parameter for equality.
        /// </summary>
        /// <param name="other">the expression to compare to</param>
        /// <returns><see langword="true"/> if the two are equal, <see langword="false"/> otherwise</returns>
        public abstract bool Equals(MathExpression other);
        /// <summary>
        /// Gets a hashcode that represents this expression.
        /// </summary>
        /// <returns>a hash code</returns>
        public abstract override int GetHashCode();
        /// <summary>
        /// Returns a string representation of the operation.
        /// </summary>
        /// <returns>a string representation of the operation</returns>
        public abstract override string ToString();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
            => obj is MathExpression e && Equals(e);
    }
}
