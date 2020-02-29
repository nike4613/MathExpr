using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class VariableExpression : MathExpression
    {
        public string Name { get; }

        public VariableExpression(string name)
        {
            Name = name;
        }

        protected internal override MathExpression Simplify()
            => this;

        public override bool Equals(MathExpression other)
            => other is VariableExpression v && (ReferenceEquals(v, this) || v.Name == Name);

        public override string ToString()
            => $"'{Name}'";

        public override int GetHashCode()
        {
            var hashCode = 890389916;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
