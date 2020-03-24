using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing a function invocation.
    /// </summary>
    public sealed class FunctionExpression : MathExpression
    {
        /// <summary>
        /// The name of the function being invoked.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The arguments to the function invocation.
        /// </summary>
        public IReadOnlyList<MathExpression> Arguments { get; }
        /// <summary>
        /// Whether or not this function invocation is a user defined function. This is notated with a prime (').
        /// </summary>
        public bool IsUserDefined { get; }

        /// <summary>
        /// The size of the expression. For this type, it is the sum of the sizes of the arguments plus one.
        /// </summary>
        public override int Size => Arguments.Sum(a => a.Size) + 1; // the function itself is only one operation as far as this is concerned

        /// <summary>
        /// Creates a new function call expression with a given name, arguments, and user defined tag.
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="args">the argumets to this funtion invocation</param>
        /// <param name="isUserDefined">whether or not this function is user defined</param>
        public FunctionExpression(string name, IReadOnlyList<MathExpression> args, bool isUserDefined)
        {
            Name = name;
            Arguments = args;
            IsUserDefined = isUserDefined;
        }

        /// <inheritdoc/>
        public override bool Equals(MathExpression other)
            => other is FunctionExpression f
            && f.Name == Name
            && Arguments.Count == f.Arguments.Count
            && Arguments.Zip(f.Arguments, (a, b) => Equals(a, b)).All(b => b);

        /// <inheritdoc/>
        public override string ToString()
            => $"{Name}{(IsUserDefined ? "'" : "")}({string.Join(", ", Arguments.Select(e => e.ToString()))})";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 2000608931;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<MathExpression>>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + IsUserDefined.GetHashCode();
            return hashCode;
        }
    }
}
