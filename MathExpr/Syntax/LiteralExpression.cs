using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public class LiteralExpression : MathExpression
    {
        public decimal Value { get; }

        public override int Size => 0; // because we don't perform any operation

        public LiteralExpression(decimal value)
            => Value = value;

        public override bool Equals(MathExpression other)
            => other is LiteralExpression l && l.Value == Value;

        protected internal override MathExpression Simplify()
            => this;

        public override string ToString()
            => Value.ToString();

        public override int GetHashCode()
        {
            var hashCode = -159790080;
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
    }
}
