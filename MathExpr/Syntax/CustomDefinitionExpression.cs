using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Syntax
{
    /// <summary>
    /// An expression representing a user function definition. This will only appear at the outermost layer of the expression tree.
    /// </summary>
    public sealed class CustomDefinitionExpression : MathExpression
    {
        /// <summary>
        /// The name of the custom function.
        /// </summary>
        public string FunctionName { get; }
        /// <summary>
        /// The list of arguments in the definition.
        /// </summary>
        public IReadOnlyList<VariableExpression> ParameterList { get; }
        /// <summary>
        /// The definition of the function.
        /// </summary>
        public MathExpression Definition { get; }
        /// <summary>
        /// The expression that uses this function, and is the result of this expression.
        /// </summary>
        public MathExpression Value { get; }

        /// <summary>
        /// The size of the expression. This is always the same as the size of <see cref="Value"/>.
        /// </summary>
        public override int Size => Value.Size;
        /// <summary>
        /// The size of the definition. This is equivalent to <c>Definition.Size</c>.
        /// </summary>
        public int DefinitionSize => Definition.Size;

        /// <summary>
        /// Creates a custom function definition from a <see cref="BinaryExpression"/> of type
        /// <see cref="BinaryExpression.ExpressionType.Equals"/> as the definition, and an expression
        /// for the value.
        /// </summary>
        /// <param name="assignExpr">the expression to deconstruct for the name and arguments</param>
        /// <param name="valueExpr">the expression to use as a <see cref="Value"/></param>
        public CustomDefinitionExpression(BinaryExpression assignExpr, MathExpression valueExpr)
        {
            if (!(assignExpr is BinaryExpression bexp) || bexp.Type != BinaryExpression.ExpressionType.Equals)
                throw new ArgumentException("Expected Equals expression");
            var fn = bexp.Left;
            if (!(fn is FunctionExpression func) || !func.IsUserDefined)
                throw new ArgumentException("Left side of definition must be a prime function");
            if (!func.Arguments.All(e => e is VariableExpression))
                throw new ArgumentException("Left side of definition cannot contain arguments with expressions");

            FunctionName = func.Name;
            ParameterList = func.Arguments.Cast<VariableExpression>().ToList();
            Definition = bexp.Right;
            Value = valueExpr;
        }

        /// <summary>
        /// Creates a custom function definition given a name, parameter list, definition, and value expression.
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="args">the arguments to the function</param>
        /// <param name="def">the definition of the function</param>
        /// <param name="val">the expression that uses the function</param>
        public CustomDefinitionExpression(string name, IReadOnlyList<VariableExpression> args, MathExpression def, MathExpression val)
        {
            FunctionName = name;
            ParameterList = args;
            Definition = def;
            Value = val;
        }

        /// <summary>
        /// Compares this expression to the parameter for equality.
        /// </summary>
        /// <param name="other">the expression to compare to</param>
        /// <returns><see langword="true"/> if the two are equal, <see langword="false"/> otherwise</returns>
        public override bool Equals(MathExpression other)
            => other is CustomDefinitionExpression cde
            && FunctionName == cde.FunctionName
            && Equals(Value, cde.Value)
            && ParameterList.Count == cde.ParameterList.Count
            && ParameterList.Zip(cde.ParameterList, (a, b) => Equals(a, b)).All(a => a);

        /// <summary>
        /// Returns a string representation of the operation.
        /// </summary>
        /// <returns>a string representation of the operation</returns>
        public override string ToString()
            => $"{FunctionName}'({string.Join(",", ParameterList)}) = {Definition}; \n{Value}";

        /// <summary>
        /// Gets a hashcode that represents this expression.
        /// </summary>
        /// <returns>a hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 312308290;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FunctionName);
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<VariableExpression>>.Default.GetHashCode(ParameterList);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Definition);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
