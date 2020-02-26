using System;
using System.Linq;
using System.Text;

namespace MathExpr.Syntax
{
    internal struct ExpressionParser
    {
        public static MathExpression ParseRoot(string s)
        {
            var tokenStream = Tokenizer.Tokenize(s);

            return null;
        }
    }
}
