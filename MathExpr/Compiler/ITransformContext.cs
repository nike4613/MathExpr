using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public interface ITransformContext<out TSettings>
    {
        TSettings Settings { get; }
    }
}
