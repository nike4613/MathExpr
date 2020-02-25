using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MathExpr.Syntax
{
    internal static class ExpressionParser
    {
        public static MathExpression ParseRoot(string s)
        {
            return null;
        }
    }

    internal static class Tokenizer
    {
        public enum TokenType
        {
            Identifier, Literal, OpenParen, CloseParen,
            Star, Slash, Plus, Minus, Exponent,
            Equals, Inequals, Less, Greater, LessEq, GreaterEq,
            Xor, And, Bang, Percent,
            None
        }

        public struct Token
        {
            public TokenType Type { get; }
            public object? Value { get; }

            public int Position { get; }
            public int Length { get; }

            public double? AsDouble => Value as double?;
            public string? AsString => Value as string;

            public Token(TokenType type, object? value, int pos, int len)
            {
                Type = type; Value = value; Position = pos; Length = len;
            }

            public override string ToString()
                => $"Token({Type}, {Value?.ToString()})";
        }

        private static bool IsIdentifierChar(char c)
            => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        private static bool IsNumberChar(char c)
            => char.IsNumber(c) || c == '.';

        public static IEnumerable<Token> Tokenize(string text)
            => TokenizeInternal(text).Where(t => t.Type != TokenType.None);
        private static IEnumerable<Token> TokenizeInternal(string text)
        {
            int tokenStart = 0;
            int tokenLen = 0;
            TokenType currentTokenType = TokenType.None;

            Token EmitToken(int i)
            {
                var value = currentTokenType switch
                {
                    TokenType.Identifier => text.Substring(tokenStart, tokenLen),
                    TokenType.Literal => (object?)double.Parse(text.Substring(tokenStart, tokenLen), CultureInfo.InvariantCulture),
                    _ => null
                };
                var tok = new Token(currentTokenType, value, tokenStart, tokenLen);
                currentTokenType = TokenType.None;
                tokenStart = i + 1;
                tokenLen = 0;
                return tok;
            }

            static Token NewToken(TokenType type, ref int i, int len = 1)
            {
                var tok = new Token(type, null, i, len);
                i += len;
                return tok;
            }

            for (int i = 0; i < text.Length;)
            {
                if (char.IsWhiteSpace(text[i]))
                    yield return EmitToken(i++);
                else if (IsIdentifierChar(text[i]))
                {
                    currentTokenType = TokenType.Identifier;
                    tokenStart = i++;
                    tokenLen = 1;
                    while (i < text.Length && IsIdentifierChar(text[i]))
                    {
                        i++;
                        tokenLen++;
                    }
                    yield return EmitToken(i);
                }
                else if (IsNumberChar(text[i]))
                {
                    currentTokenType = TokenType.Literal;
                    tokenStart = i++;
                    tokenLen = 1;
                    while (i < text.Length && IsNumberChar(text[i]))
                    {
                        i++;
                        tokenLen++;
                    }
                    yield return EmitToken(i);
                }
                else
                {
                    switch (text[i])
                    {
                        case '(':
                            yield return NewToken(TokenType.OpenParen, ref i);
                            break;
                        case ')':
                            yield return NewToken(TokenType.CloseParen, ref i);
                            break;
                        case '*':
                            yield return NewToken(TokenType.Star, ref i);
                            break;
                        case '/':
                            yield return NewToken(TokenType.Slash, ref i);
                            break;
                        case '+':
                            yield return NewToken(TokenType.Plus, ref i);
                            break;
                        case '-':
                            yield return NewToken(TokenType.Minus, ref i);
                            break;
                        case '^':
                            if (text[i + 1] == '^')
                                yield return NewToken(TokenType.Xor, ref i, 2);
                            else
                                yield return NewToken(TokenType.Exponent, ref i);
                            break;
                        case '=':
                            yield return NewToken(TokenType.Equals, ref i);
                            break;
                        case '%':
                            yield return NewToken(TokenType.Percent, ref i);
                            break;
                        case '&':
                            yield return NewToken(TokenType.And, ref i);
                            break;
                        case '!':
                            if (text[i + 1] == '=')
                                yield return NewToken(TokenType.Inequals, ref i, 2);
                            else
                                yield return NewToken(TokenType.Bang, ref i);
                            break;
                        case '<':
                            if (text[i + 1] == '=')
                                yield return NewToken(TokenType.LessEq, ref i, 2);
                            else
                                yield return NewToken(TokenType.Less, ref i);
                            break;
                        case '>':
                            if (text[i + 1] == '=')
                                yield return NewToken(TokenType.GreaterEq, ref i, 2);
                            else
                                yield return NewToken(TokenType.Greater, ref i);
                            break;
                    }
                }
            }
        }
    }
}
