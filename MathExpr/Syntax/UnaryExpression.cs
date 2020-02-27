using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class UnaryExpression : MathExpression
    {
        public enum ExpressionType
        {
            Negate, Not, Factorial
        }

        public ExpressionType Type { get; }
        public MathExpression Argument { get; }

        public UnaryExpression(ExpressionType type, MathExpression arg)
        {
            Type = type;
            Argument = arg;
        }

        public override bool Equals(MathExpression other)
            => other is UnaryExpression e 
            && (ReferenceEquals(this, e) 
                || (Type == e.Type && Argument == e.Argument));

        protected internal override MathExpression Simplify()
        {
            return new UnaryExpression(Type, Argument.Simplify());
        }

        public override string ToString()
            => $"({Type} {Argument.ToString()}";
    }
}
