using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class Extras
    {
        [Theory]
        [InlineData(BinaryExpression.ExpressionType.Add, false, false)]
        [InlineData(BinaryExpression.ExpressionType.And, true, false)]
        [InlineData(BinaryExpression.ExpressionType.Divide, false, false)]
        [InlineData(BinaryExpression.ExpressionType.Equals, false, true)]
        [InlineData(BinaryExpression.ExpressionType.Greater, false, true)]
        [InlineData(BinaryExpression.ExpressionType.GreaterEq, false, true)]
        [InlineData(BinaryExpression.ExpressionType.Inequals, false, true)]
        [InlineData(BinaryExpression.ExpressionType.Less, false, true)]
        [InlineData(BinaryExpression.ExpressionType.LessEq, false, true)]
        [InlineData(BinaryExpression.ExpressionType.Modulo, false, false)]
        [InlineData(BinaryExpression.ExpressionType.Multiply, false, false)]
        [InlineData(BinaryExpression.ExpressionType.NAnd, true, false)]
        [InlineData(BinaryExpression.ExpressionType.NOr, true, false)]
        [InlineData(BinaryExpression.ExpressionType.Or, true, false)]
        [InlineData(BinaryExpression.ExpressionType.Power, false, false)]
        [InlineData(BinaryExpression.ExpressionType.Subtract, false, false)]
        [InlineData(BinaryExpression.ExpressionType.XNor, true, false)]
        [InlineData(BinaryExpression.ExpressionType.Xor, true, false)]
        public void BinaryExpressionTypeChecks(BinaryExpression.ExpressionType type, bool isBool, bool isComp)
        {
            Assert.Equal(isBool, type.IsBooleanType());
            Assert.Equal(isComp, type.IsComparisonType());

            Assert.Equal(isBool, type.IsBooleanType());
            Assert.Equal(isComp, type.IsComparisonType());
        }
    }
}
