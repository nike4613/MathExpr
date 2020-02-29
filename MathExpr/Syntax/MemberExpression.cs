using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class MemberExpression : MathExpression
    {
        public MathExpression Target { get; }
        public string MemberName { get; }

        public override int Size => Target.Size + 1;

        public MemberExpression(MathExpression target, string member)
        {
            Target = target;
            MemberName = member;
        }

        public override bool Equals(MathExpression other)
            => other is MemberExpression mem
            && MemberName == mem.MemberName
            && Equals(Target, mem.Target);

        public override string ToString()
            => $"{Target}.{MemberName}";

        protected internal override MathExpression Simplify()
            => new MemberExpression(Target.Simplify(), MemberName);

        public override int GetHashCode()
        {
            var hashCode = 1134248118;
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MemberName);
            return hashCode;
        }
    }
}
