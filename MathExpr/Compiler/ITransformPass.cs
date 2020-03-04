﻿using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Compiler
{
    public interface ITransformPass<in TContext, TFrom, TTo>
    {
        TTo ApplyTo(TFrom expr, TContext context);
    }
}