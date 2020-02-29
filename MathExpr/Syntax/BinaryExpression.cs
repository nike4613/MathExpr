using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class BinaryExpression : MathExpression
    {
        public enum ExpressionType 
        {
            Add, Subtract, Multiply, Divide, Modulo, Exponent,
            And, NAnd, Or, NOr, Xor, XNor,
            Equals, Inequals, Less, Greater, LessEq, GreaterEq,
        }
        
        public ExpressionType Type { get; }
        public MathExpression Left { get; }
        public MathExpression Right { get; }

        public BinaryExpression(MathExpression left, ExpressionType type, MathExpression right)
        {
            Type = type;
            Left = left;
            Right = right;
        }

        public override bool Equals(MathExpression other)
            => other is BinaryExpression e
            && (ReferenceEquals(this, e) 
                || (Type == e.Type && Equals(Left, e.Left) && Equals(Right, e.Right)));

        protected internal override MathExpression Simplify()
        {
            return new BinaryExpression(Left.Simplify(), Type, Right.Simplify());
        }

        public override string ToString()
            => $"({Left.ToString()} {Type} {Right.ToString()})";

        public override int GetHashCode()
        {
            var hashCode = 1099731784;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Left);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Right);
            return hashCode;
        }
    }
}
