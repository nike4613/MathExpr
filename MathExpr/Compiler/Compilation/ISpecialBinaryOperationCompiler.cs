using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    /// <summary>
    /// A compiler that can compile a special binary operation.
    /// </summary>
    public interface ISpecialBinaryOperationCompiler
    {
        /// <summary>
        /// Attempts to compile the implemented binary operation.
        /// </summary>
        /// <param name="left">the left hand parameter</param>
        /// <param name="right">the right hand parameter</param>
        /// <param name="result">the compiled expression</param>
        /// <returns><see langword="true"/> if this compiler could compile the given arguments, <see langword="false"/>
        /// otherwise.</returns>
        bool TryCompile(Expression left, Expression right, [MaybeNullWhen(false)] out Expression result);
    }
}
