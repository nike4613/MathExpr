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

        private BinaryExpression(ExpressionType type, IReadOnlyList<MathExpression> args)
        {
            if (args.Count < 2)
                throw new ArgumentException("A BinaryExpression must have at least 2 arguments", nameof(args));
            Type = type;
            Arguments = args;
        }

        public override bool Equals(MathExpression other)
            => other is BinaryExpression e
            && Arguments.Count == e.Arguments.Count
            && Arguments.Zip(e.Arguments, (a, b) => Equals(a, b)).All(b => b);

        protected internal override MathExpression Simplify()
        {
            var list = Arguments.Select(a => a.Simplify()).ToList();
            switch (Type)
            {
                case ExpressionType.Add:
                case ExpressionType.Multiply:
                case ExpressionType.And:
                case ExpressionType.Or:
                    bool IsCombinableExpression(MathExpression e)
                        => e is BinaryExpression ex && ex.Type == Type;
                    if (list.Any(IsCombinableExpression))
                    {
                        foreach (var ex in list.ToArray().Where(IsCombinableExpression).Cast<BinaryExpression>())
                        {
                            list.Remove(ex);
                            list.AddRange(ex.Arguments);
                        }
                    }
                    break;
            }
            static bool IsValueExpression(MathExpression e)
                => e is LiteralExpression;
            if (list.Count(IsValueExpression) > 1)
            {
                var arr = list.Where(IsValueExpression).Cast<LiteralExpression>().ToArray();
                var sum = arr.Select(l => l.Value).Aggregate(0m, Type switch
                {
                    ExpressionType.Add          => (a, b) => a + b,
                    ExpressionType.Subtract     => (a, b) => a - b,
                    ExpressionType.Multiply     => (a, b) => a * b,
                    ExpressionType.Divide       => (a, b) => a / b,
                    ExpressionType.Modulo       => (a, b) => a % b,
                    ExpressionType.Exponent     => (a, b) => decimal.MinValue, // TODO: because exponents are hard, and i'm working with Decimal which doesn't natively support it
                    // x^n can be represented as sigma(v=0 -> inf, (n^v * log(x)^v) / v!)
                    ExpressionType.And          => (a, b) => a != 0 && b != 0 ? 1 : 0,
                    ExpressionType.NAnd         => (a, b) => a != 0 && b != 0 ? 0 : 1,
                    ExpressionType.Or           => (a, b) => a != 0 || b != 0 ? 1 : 0,
                    ExpressionType.NOr          => (a, b) => a != 0 || b != 0 ? 0 : 1,
                    ExpressionType.Xor          => (a, b) => a != 0 ^ b != 0 ? 1 : 0,
                    ExpressionType.XNor         => (a, b) => a != 0 ^ b != 0 ? 0 : 1,
                    ExpressionType.Equals       => (a, b) => a == b ? 1 : 0,
                    ExpressionType.Inequals     => (a, b) => a == b ? 0 : 1,
                    ExpressionType.Less         => (a, b) => a < b ? 1 : 0,
                    ExpressionType.Greater      => (a, b) => a > b ? 1 : 0,
                    ExpressionType.LessEq       => (a, b) => a <= b ? 1 : 0,
                    ExpressionType.GreaterEq    => (a, b) => a >= b ? 1 : 0,
                    _ => throw new InvalidOperationException("Attempting to aggregate unknown operation")
                });
                foreach (var e in arr)
                    list.Remove(e);
                list.Add(new LiteralExpression(sum));
            }
            if (list.Count < 2) return list.First();
            return new BinaryExpression(Type, list);
        }

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
