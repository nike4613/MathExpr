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

        public override int Size => Argument.Size + 1;

        public UnaryExpression(ExpressionType type, MathExpression arg)
        {
            Type = type;
            Argument = arg;
        }

        public override bool Equals(MathExpression other)
            => other is UnaryExpression e 
            && Type == e.Type 
            && Equals(Argument, e.Argument);

        protected internal override MathExpression Simplify()
        {
            return new UnaryExpression(Type, Argument.Simplify());
        }
        protected internal override MathExpression Reduce()
        {
            var arg = Argument.Reduce();
            if (arg is LiteralExpression l)
            {
                switch (Type)
                {
                    case ExpressionType.Negate:
                        return new LiteralExpression(-l.Value);
                    case ExpressionType.Not:
                        return new LiteralExpression(l.Value != 0 ? 0 : 1);
                    case ExpressionType.Factorial:
                        // TODO: because i will implement factorial partially with exponents
                        break;
                }
            }

            return new UnaryExpression(Type, Argument.Reduce());
        }

        public override string ToString()
            => $"({Type} {Argument.ToString()}";

        public override int GetHashCode()
        {
            var hashCode = -850124847;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Argument);
            return hashCode;
        }
    }
}
