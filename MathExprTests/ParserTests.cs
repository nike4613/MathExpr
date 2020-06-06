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
            var tokens = Tokenizer.Tokenize("a+b*c/d^ehij  % k %( 3.442*ident) ^^ y & y \"haha this is a \\\" string literal &^\"");
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
                new Token(TokenType.String, "haha this is a \" string literal &^", 43, 37)
            };

            foreach (var (actual, expected) in tokens.Zip(expect))
            {
                Assert.Equal(expected, actual);
            }
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
        [InlineData("f'(x, y) = x^2 + y - 1; f'(2, 3 + z)^2", true)]
        [InlineData("vec3(a, b, c).x", true)] // ideally this should be able to simplify to just 'a'
        [InlineData("(vec3(a, b, c) * 13 + vec3(1,2,3)).z", true)]
        public void ParseString(string input, bool valid)
        {
            try
            {
                _ = MathExpression.Parse(input);
                Assert.True(valid, "Parser did not throw when it was supposed to");
            }
            catch (SyntaxException e)
            {
                _ = e.ToString();
                Assert.False(valid, "Parser threw when it was not supposed to");
            }
        }
    }
}
