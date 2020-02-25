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
            var tokens = Tokenizer.Tokenize("a+b*c/d^ehij  % k %( 3.442*ident) ^^ y & y");
            var expect = new[]
            {
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "a", 0, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Plus, null, 1, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "b", 2, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Star, null, 3, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "c", 4, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Slash, null, 5, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "d", 6, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Exponent, null, 7, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "ehij", 8, 4),
                new Tokenizer.Token(Tokenizer.TokenType.Percent, null, 14, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "k", 16, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Percent, null, 18, 1),
                new Tokenizer.Token(Tokenizer.TokenType.OpenParen, null, 19, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Literal, 3.442d, 21, 5),
                new Tokenizer.Token(Tokenizer.TokenType.Star, null, 26, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "ident", 27, 5),
                new Tokenizer.Token(Tokenizer.TokenType.CloseParen, null, 32, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Xor, null, 34, 2),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "y", 37, 1),
                new Tokenizer.Token(Tokenizer.TokenType.And, null, 39, 1),
                new Tokenizer.Token(Tokenizer.TokenType.Identifier, "y", 41, 1),
            };

            Assert.Equal(expect, tokens);
        }

    }
}
