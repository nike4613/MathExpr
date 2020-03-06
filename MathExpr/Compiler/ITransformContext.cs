using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public interface ITransformContext<out TSettings, TFrom, TTo>
    {
        TSettings Settings { get; }

        TTo Transform(TFrom from);
    }
}
