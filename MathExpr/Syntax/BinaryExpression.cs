using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public MathExpression Left => Arguments[0];
        public MathExpression Right => Arguments[1];

        public IReadOnlyList<MathExpression> Arguments { get; }

        public override int Size => Arguments.Sum(a => a.Size) + (Arguments.Count - 1);

        public BinaryExpression(MathExpression left, ExpressionType type, MathExpression right)
        {
            Type = type;
            Arguments = new List<MathExpression> { left, right };
        }

        public BinaryExpression(ExpressionType type, IReadOnlyList<MathExpression> args)
        {
            if (args.Count < 2)
                throw new ArgumentException("A BinaryExpression must have at least 2 arguments", nameof(args));
            if (Arguments.Count > 2)
                switch (type)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Multiply:
                    case ExpressionType.And:
                    case ExpressionType.Or:
                        break;
                    default:
                        throw new ArgumentException("Can only have more than 2 arguments when the type is commutative");
                }
            Type = type;
            Arguments = args;
        }

        public override bool Equals(MathExpression other)
            => other is BinaryExpression e
            && Arguments.Count == e.Arguments.Count
            && Arguments.Zip(e.Arguments, (a, b) => Equals(a, b)).All(b => b);
        // TODO: make Equals not care about order in some cases

        public override string ToString()
            => $"({string.Join($" {Type} ", Arguments)})";

        public override int GetHashCode()
        {
            var hashCode = 1099731784;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<MathExpression>>.Default.GetHashCode(Arguments);
            return hashCode;
        }

    }
}
