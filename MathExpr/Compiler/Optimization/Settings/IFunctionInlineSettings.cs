using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler.Optimization.Settings
{
    public interface IFunctionInlineSettings
    {
        bool ShouldInline { get; }
        int DoNotInlineAfterSize { get; }
    }
}
