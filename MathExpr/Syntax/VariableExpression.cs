﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class VariableExpression : MathExpression
    {
        public string Name { get; }

        public override int Size => 1; // because it is one operation to load

        public VariableExpression(string name)
        {
            Name = name;
        }

        public override bool Equals(MathExpression other)
            => other is VariableExpression v && v.Name == Name;

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
