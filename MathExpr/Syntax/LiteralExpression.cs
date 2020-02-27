using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public class LiteralExpression : MathExpression
    {
        public double Value { get; }
        public LiteralExpression(double value)
            => Value = value;

        public override bool Equals(MathExpression other)
            => other is LiteralExpression l && l.Value == Value;

        protected internal override MathExpression Simplify()
            => this;

        public override string ToString()
            => Value.ToString();
    }
}
