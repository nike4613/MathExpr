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

        public FunctionExpression(string name, IReadOnlyList<MathExpression> args)
        {
            Name = name;
            Arguments = args;
        }

        protected internal override MathExpression Simplify()
            => this; // never simplifies (at this stage)

        public override bool Equals(MathExpression other)
            => other is FunctionExpression f
            && (ReferenceEquals(f, this)
                || (f.Name == Name && Arguments.Zip(f.Arguments, (a, b) => a.Equals(b)).All(b => b)));

        public override string ToString()
            => $"{Name}({string.Join(", ", Arguments.Select(e => e.ToString()))})";
    }
}
