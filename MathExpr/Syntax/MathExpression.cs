using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public abstract class MathExpression
    {
        internal MathExpression() { }

        public static MathExpression Parse(string expr)
            => ExpressionParser.ParseRoot(expr);

        protected internal abstract MathExpression Simplify();

        public abstract bool Equals(MathExpression other);
    }
}
