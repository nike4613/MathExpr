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
                || (Type == e.Type && Left == e.Left && Right == Right));

        protected internal override MathExpression Simplify()
        {
            return new BinaryExpression(Left.Simplify(), Type, Right.Simplify());
        }

        public override string ToString()
            => $"({Left.ToString()} {Type} {Right.ToString()})";
    }
}
