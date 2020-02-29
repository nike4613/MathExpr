﻿using System;
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
                new Token(TokenType.Identifier, "a", 0, 1),
                new Token(TokenType.Plus, null, 1, 1),
                new Token(TokenType.Identifier, "b", 2, 1),
                new Token(TokenType.Star, null, 3, 1),
                new Token(TokenType.Identifier, "c", 4, 1),
                new Token(TokenType.Slash, null, 5, 1),
                new Token(TokenType.Identifier, "d", 6, 1),
                new Token(TokenType.Exponent, null, 7, 1),
                new Token(TokenType.Identifier, "ehij", 8, 4),
                new Token(TokenType.Percent, null, 14, 1),
                new Token(TokenType.Identifier, "k", 16, 1),
                new Token(TokenType.Percent, null, 18, 1),
                new Token(TokenType.OpenParen, null, 19, 1),
                new Token(TokenType.Literal, 3.442m, 21, 5),
                new Token(TokenType.Star, null, 26, 1),
                new Token(TokenType.Identifier, "ident", 27, 5),
                new Token(TokenType.CloseParen, null, 32, 1),
                new Token(TokenType.Xor, null, 34, 2),
                new Token(TokenType.Identifier, "y", 37, 1),
                new Token(TokenType.And, null, 39, 1),
                new Token(TokenType.Identifier, "y", 41, 1),
            };

            Assert.Equal(expect, tokens);
        }

        [Theory]
        [InlineData("a + b + c - 3 * d ^ e * (f - g)", true)]
        [InlineData("a + b + c - 3 * d ^ e * (f - g", false)]
        [InlineData("a + b + c - 3 * d ^ e * f - g)", false)]
        [InlineData("a + b + c - 3 * d ^ e * {f - g)", false)]
        [InlineData("a + b + c - 3 * d ^ e * f - g", true)]
        [InlineData("a+b*c/d^ehij  % k %( 3.442*ident) ^^ y & y", true)]
        [InlineData("x = y + 2 ~^ y * 2 > z", true)]
        [InlineData("func'(a, b) = 14.3 * a * b!", true)]
        [InlineData("5!", true)]
        [InlineData("-5!", true)]
        [InlineData("(x+1)!", true)]
        [InlineData("-(x+1)!", true)]
        [InlineData("(x = y + 2 ~^ y * 2 > z) * (x+1)!", true)]
        public void ParseString(string input, bool valid)
        {
            try
            {
                _ = ExpressionParser.ParseRoot(input);
                Assert.True(valid, "Parser did not throw when it was supposed to");
            }
            catch (SyntaxException e)
            {
                Assert.False(valid, "Parser threw when it was not supposed to");
            }
        }
    }
}
