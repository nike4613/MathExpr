using System.Collections.Generic;
using System.Globalization;

namespace MathExpr.Syntax
{
    public enum TokenType
    {
        Identifier, Literal, OpenParen, CloseParen,
        Star, Slash, Plus, Minus, Exponent,
        Equals, Inequals, Less, Greater, LessEq, GreaterEq,
        Xor, And, Bang, Percent,

        Error
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

        public override bool Equals(object obj)
            => obj is Token t && this == t;
        public static bool operator ==(Token a, Token b)
            => a.Type == b.Type && Equals(a.Value, b.Value);
        public static bool operator !=(Token a, Token b)
            => !(a == b);

        public override string ToString()
            => $"Token({Type}, {Value?.ToString()})";

        public override int GetHashCode()
        {
            var hashCode = 9789246;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object?>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }
    }

    internal static class Tokenizer
    {

        private static bool IsIdentifierChar(char c)
            => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        private static bool IsNumberChar(char c)
            => char.IsNumber(c) || c == '.';

        public static IEnumerable<Token> Tokenize(string text)
        {
            int tokenStart;
            int tokenLen;
            TokenType currentTokenType;

            Token EmitToken()
            {
                var value = currentTokenType switch
                {
                    TokenType.Identifier => text.Substring(tokenStart, tokenLen),
                    TokenType.Literal => (object?)double.Parse(text.Substring(tokenStart, tokenLen), CultureInfo.InvariantCulture),
                    _ => null
                };
                var tok = new Token(currentTokenType, value, tokenStart, tokenLen);
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
                    i++;
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
                    yield return EmitToken();
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
                    yield return EmitToken();
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
                        default:
                            yield return new Token(TokenType.Error, "Unexpected character", i++, 1);
                            break;
                    }
                }
            }
        }
    }
}
