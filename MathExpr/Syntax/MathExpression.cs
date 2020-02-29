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

        /// <summary>
        /// The size of the expression; that is, the number of operations it contains
        /// </summary>
        public abstract int Size { get; }
        public abstract bool Equals(MathExpression other);
        public abstract override int GetHashCode();
        public abstract override string ToString();

        public override bool Equals(object obj)
            => obj is MathExpression e && Equals(e);
    }
}
