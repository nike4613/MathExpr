using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

using MathExpr.Syntax;
using System.Linq;

namespace MathExprTests
{
    public class ParserTests
    {
        [Fact]
        public void TokenizeString()
        {
            var tokens = Tokenizer.Tokenize("a+b*c/d^ehij  % k %( 3.442*ident) ^^ y & y").ToArray();
        }
    }
}
