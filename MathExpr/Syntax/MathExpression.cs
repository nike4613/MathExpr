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
        /// <param name="saveText">whether or not to save the original input text in the parsed tokens</param>
        /// <returns>the expression tree</returns>
        /// <exception cref="SyntaxException">when there is a syntax error</exception>
        /// <exception cref="AggregateException">when there are multiple identified syntax errors</exception>
        public static MathExpression Parse(string expr, bool saveText = true)
            => ExpressionParser.ParseRoot(expr, saveText);

        private Token? identificationToken;
        /// <summary>
        /// Gets a token that can be used to identify where in the source this expression exists.
        /// </summary>
        public virtual Token? Token => identificationToken;

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

        internal virtual void WithTokenInternal(Token? tok)
            => identificationToken = tok;
    }

    /// <summary>
    /// Extension methods for <see cref="MathExpression"/>s.
    /// </summary>
    public static class MathExpressionExtensions
    {
        /// <summary>
        /// Associates a token with an expression in a way that is non-intrusive to implementations.
        /// </summary>
        /// <typeparam name="T">the type of the expression</typeparam>
        /// <param name="expr">the expression to associate with</param>
        /// <param name="tok">the token to associate</param>
        /// <returns><paramref name="expr"/></returns>
        public static T WithToken<T>(this T expr, Token? tok) where T : MathExpression
        {
            expr.WithTokenInternal(tok);
            return expr;
        }
    }
}
