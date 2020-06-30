using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    /// <summary>
    /// An exception thrown during the compilation of a <see cref="MathExpression"/>.
    /// </summary>
    public class CompilationException : Exception
    {
        /// <summary>
        /// The expression that caused this exception.
        /// </summary>
        public MathExpression? Location { get; } = null;

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> wrapping another exception at a given location.
        /// </summary>
        /// <param name="at">the expression that caused the exception</param>
        /// <param name="innerException">the inner exception</param>
        public CompilationException(MathExpression at, Exception innerException) : this("An error ocurred while compiling an expression", at, innerException)
        {
        }

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> with a given message and location.
        /// </summary>
        /// <param name="message">the message to use for the exception</param>
        /// <param name="at">the expression that caused the exception</param>
        public CompilationException(string message, MathExpression at) : base(message)
        {
            Location = at;
        }

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> with a message wrapping another exception at a given location.
        /// </summary>
        /// <param name="message">the message to associate with this exception</param>
        /// <param name="at">the expression that caused the exception</param>
        /// <param name="innerException">the inner exception</param>
        public CompilationException(string message, MathExpression at, Exception innerException) : base(message, innerException)
        {
            Location = at;
        }

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> with no associated information.
        /// </summary>
        public CompilationException()
        {
        }

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> with a message.
        /// </summary>
        /// <param name="message">the message to associate with the exception</param>
        public CompilationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a <see cref="CompilationException"/> that wraps another exception with a given message.
        /// </summary>
        /// <param name="message">the message to associate with the exceptiom</param>
        /// <param name="innerException">the inner exception</param>
        public CompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
            => (Location?.Token?.FormatTokenLocation() ?? "") + base.ToString();
    }
}
