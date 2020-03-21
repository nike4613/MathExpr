using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression that represents a member access.
    /// </summary>
    public sealed class MemberExpression : MathExpression
    {
        /// <summary>
        /// The target of the member access.
        /// </summary>
        public MathExpression Target { get; }
        /// <summary>
        /// The name of the member to access/
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// The size of the expression. This is always the size of the target expression plus one.
        /// </summary>
        public override int Size => Target.Size + 1;

        /// <summary>
        /// Constructs a new <see cref="MemberExpression"/> for a given target and member name.
        /// </summary>
        /// <param name="target">the target of the access</param>
        /// <param name="member">the name of the member to access</param>
        public MemberExpression(MathExpression target, string member)
        {
            Target = target;
            MemberName = member;
        }

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is MemberExpression mem
            && MemberName == mem.MemberName
            && Equals(Target, mem.Target);

        /// <inheritdoc/>
        public override string ToString()
            => $"{Target}.{MemberName}";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 1134248118;
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MemberName);
            return hashCode;
        }

    }
}
