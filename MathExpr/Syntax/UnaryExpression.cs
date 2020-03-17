﻿using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Syntax
{
    public sealed class UnaryExpression : MathExpression
    {
        public enum ExpressionType
        {
            Negate, Not, Factorial
        }

        public ExpressionType Type { get; }
        public MathExpression Argument { get; }

        public override int Size => Argument.Size + 1;

        public UnaryExpression(ExpressionType type, MathExpression arg)
        {
            Type = type;
            Argument = arg;
        }

        public override bool Equals(MathExpression other)
            => other is UnaryExpression e 
            && Type == e.Type 
            && Equals(Argument, e.Argument);

        public override string ToString()
            => $"({Type} {Argument})";

        public override int GetHashCode()
        {
            var hashCode = -850124847;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpression>.Default.GetHashCode(Argument);
            return hashCode;
        }
    }
}
