using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class FunctionExpression : MathExpression
    {
        public string Name { get; }
        public IReadOnlyList<MathExpression> Arguments { get; }
        public bool IsPrime { get; }

        public override int Size => Arguments.Sum(a => a.Size) + 1; // the function itself is only one operation as far as this is concerned

        public FunctionExpression(string name, IReadOnlyList<MathExpression> args, bool isPrime)
        {
            Name = name;
            Arguments = args;
            IsPrime = isPrime;
        }

        protected internal override MathExpression Simplify()
            => this; // never simplifies (at this stage)

        public override bool Equals(MathExpression other)
            => other is FunctionExpression f
            && (ReferenceEquals(f, this)
                || (f.Name == Name && Arguments.Zip(f.Arguments, (a, b) => Equals(a, b)).All(b => b)));

        public override string ToString()
            => $"{Name}{(IsPrime ? "'" : "")}({string.Join(", ", Arguments.Select(e => e.ToString()))})";

        public override int GetHashCode()
        {
            var hashCode = 2000608931;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<MathExpression>>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + IsPrime.GetHashCode();
            return hashCode;
        }
    }
}
