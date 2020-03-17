using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation
{
    public interface ISpecialBinaryOperationCompiler
    {
        bool TryCompile(Expression left, Expression right, [MaybeNullWhen(false)] out Expression result);
    }
}
