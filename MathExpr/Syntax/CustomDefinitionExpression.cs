using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class CustomDefinitionExpression : MathExpression
    {
        public string FunctionName { get; }
        public IReadOnlyList<VariableExpression> ArgumentList { get; }
        public MathExpression Definition { get; }
        public MathExpression Value { get; }

        public override int Size => Value.Size;
        public int DefinitionSize => Definition.Size;

        public CustomDefinitionExpression(MathExpression assignExpr, MathExpression valueExpr)
        {
            if (!(assignExpr is BinaryExpression bexp) || bexp.Type != BinaryExpression.ExpressionType.Equals)
                throw new ArgumentException("Expected Equals expression");
            var fn = bexp.Left;
            if (!(fn is FunctionExpression func) || !func.IsPrime)
                throw new ArgumentException("Left side of definition must be a prime function");
            if (!func.Arguments.All(e => e is VariableExpression))
                throw new ArgumentException("Left side of definition cannot contain arguments with expressions");

            FunctionName = func.Name;
            ArgumentList = func.Arguments.Cast<VariableExpression>().ToList();
            Definition = bexp.Right;
            Value = valueExpr;
        }

        private CustomDefinitionExpression(string name, IReadOnlyList<VariableExpression> args, MathExpression def, MathExpression val)
        {
            FunctionName = name;
            ArgumentList = args;
            Definition = def;
            Value = val;
        }

        public override bool Equals(MathExpression other)
            => other is CustomDefinitionExpression cde
            && FunctionName == cde.FunctionName
            && Equals(Value, cde.Value)
            && ArgumentList.Zip(cde.ArgumentList, (a, b) => Equals(a, b)).All(a => a);

        public override string ToString()
            => $"{FunctionName}'({string.Join(",", ArgumentList)}) = {Definition}; \n{Value}";

        protected internal override MathExpression Simplify()
            => new CustomDefinitionExpression(FunctionName, ArgumentList, Definition.Simplify(), Value.Simplify());

        public override int GetHashCode()
        {
            var hashCode = 312308290;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FunctionName);
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<VariableExpression>>.Default.GetHashCode(ArgumentList);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Definition);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
