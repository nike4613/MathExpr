using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    /// <summary>
    /// Settings to control user inlining.
    /// </summary>
    public interface IFunctionInlineSettings
    {
        /// <summary>
        /// Whether or not user functions should be inlined.
        /// </summary>
        /// <remarks>
        /// Currently, the only implemented compiler backed does not support un-inlined user
        /// functions, and so this should always be <see langword="true"/>.
        /// </remarks>
        bool ShouldInline { get; }
        /// <summary>
        /// The largest ize of user function that will be inlined.
        /// </summary>
        int DoNotInlineAfterSize { get; }
    }
}
