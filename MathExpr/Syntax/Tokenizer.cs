using MathExpr.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace MathExpr.Syntax
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class TokenDescAttribute : Attribute
    {
        public string Description { get; }
        public TokenDescAttribute(string desc) 
        {
            Description = desc;
        }
    }

    /// <summary>
    /// The type of a token.
    /// </summary>
    [SuppressMessage("Documentation", "CS1591", Justification = "The names are self-explanatory.")]
    public enum TokenType
    {
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
        [TokenDesc(@"[A-Za-z_]([A-Za-z_]|\d)*")] Identifier,
        [TokenDesc(@"\d+(\.\d+)?")] Literal,
        [TokenDesc("(")]            OpenParen,
        [TokenDesc(")")]            CloseParen,
        [TokenDesc(",")]            Comma,
        [TokenDesc("*")]            Star,
        [TokenDesc("/")]            Slash,
        [TokenDesc("+")]            Plus,
        [TokenDesc("-")]            Minus,
        [TokenDesc("^")]            Exponent,
        [TokenDesc("=")]            Equals,
        [TokenDesc("~=")]           Inequals,
        [TokenDesc("<")]            Less,
        [TokenDesc(">")]            Greater,
        [TokenDesc("<=")]           LessEq,
        [TokenDesc(">=")]           GreaterEq,
        [TokenDesc("^^")]           Xor,
        [TokenDesc("~^")]           XNor,
        [TokenDesc("&")]            And,
        [TokenDesc("~&")]           NAnd,
        [TokenDesc("|")]            Or,
        [TokenDesc("~|")]           NOr,
        [TokenDesc("!")]            Factorial,
        [TokenDesc("%")]            Percent,
        [TokenDesc("~")]            Not,
        [TokenDesc("'")]            Prime,
        [TokenDesc(";")]            Semicolon,
        [TokenDesc(".")]            Period,
        [TokenDesc(@""".*""")]      String,
#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// A token type that is emitted only when there is a parser error.
        /// </summary>
        /// <remarks>
        /// This is emitted as a token in order to allow the parser to continue even when it finds
        /// something it doesn't expect. The error message will be found in the token's <see cref="Token.AsString"/>
        /// property.
        /// </remarks>
        Error,
    }

    /// <summary>
    /// A parser token.
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// The type of the token.
        /// </summary>
        public TokenType Type { get; }
        /// <summary>
        /// The value contained in the token, if any.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// The starting position of the token in the input string.
        /// </summary>
        public int Position { get; }
        /// <summary>
        /// The length of the token in the input string.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The input text that this token was parsed from, if that information is avaliable.
        /// </summary>
        public string? InputText { get; }

        /// <summary>
        /// The value contained in this token as a <see cref="decimal"/>.
        /// </summary>
        public decimal? AsDecimal => Value as decimal?;
        /// <summary>
        /// The value contained in this token as a <see cref="string"/>.
        /// </summary>
        public string? AsString => Value as string;

        /// <summary>
        /// Constructs a new token of the given type with the given value, position,
        /// and length.
        /// </summary>
        /// <param name="type">the type of token being constructed</param>
        /// <param name="value">the value to store in the token</param>
        /// <param name="pos">the position of the token in the input string</param>
        /// <param name="len">the length of the token</param>
        public Token(TokenType type, object? value, int pos, int len) : this(type, value, pos, len, null) 
        {
        }

        /// <summary>
        /// Constructs a new token of the given type with the given value, position,
        /// and length, along with the original input text for debugging.
        /// </summary>
        /// <param name="type">the type of token being constructed</param>
        /// <param name="value">the value to store in the token</param>
        /// <param name="pos">the position of the token in the input string</param>
        /// <param name="len">the length of the token</param>
        /// <param name="inputText">the text that this token was parsed from</param>
        public Token(TokenType type, object? value, int pos, int len, string? inputText)
        {
            Type = type; Value = value; Position = pos; 
            Length = len; InputText = inputText;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
            => obj is Token t && this == t;
        /// <summary>
        /// Compares two tokens for equality.
        /// </summary>
        /// <param name="a">the first token to compare</param>
        /// <param name="b">the second token to compare</param>
        /// <returns>whether or not the tokens are equal</returns>
        public static bool operator ==(Token a, Token b)
            => a.Type == b.Type && Equals(a.Value, b.Value);
        /// <summary>
        /// Tests if two tokens are inequal.
        /// </summary>
        /// <param name="a">the first token to compare</param>
        /// <param name="b">the second token to compare</param>
        /// <returns>whether or not the tokens are inequal</returns>
        public static bool operator !=(Token a, Token b)
            => !(a == b);

        /// <summary>
        /// Returns a string represenation of this token.
        /// </summary>
        /// <returns>the string representation of the token</returns>
        public override string ToString()
            => $"Token({Type}, {Value?.ToString()})";

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 9789246;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object?>.Default.GetHashCode(Value!);
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Constructs an ASCII art-style representation of this token and its location.
        /// </summary>
        /// <returns>a string representing the token and its location in the original string</returns>
        public string FormatTokenLocation()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"at {Position} (token type {Type})");

            if (InputText == null)
                return sb.ToString();

            var lineNo = InputText.CountLinesBefore(Position) + 1;
            var lineStart = InputText.FindLineBreakBefore(Position);
            var lineEnd = InputText.FindLineBreakAfter(Position);

            var line = InputText.Substring(lineStart, lineEnd - lineStart);

            var lineNoStr = $" {lineNo} ";

            sb.Append('-', lineNoStr.Length)
              .AppendLine("+")
              .Append(lineNoStr)
              .Append("|")
              .AppendLine(line)
              .Append('-', lineNoStr.Length)
              .Append('+')
              .Append(' ', Position - lineStart)
              .Append('^')
              .Append('~', Length - 1)
              .AppendLine()
              .Append(' ', lineNoStr.Length + (Position - lineStart) + 1)
              .AppendLine("here");

            return sb.ToString();
        }
    }

    internal static class Tokenizer
    {

        private static bool IsIdentifierChar(char c)
            => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
        private static bool IsNumberChar(char c)
            => char.IsNumber(c);

        public static IEnumerable<Token> Tokenize(string text, bool saveText = true)
        {
            int tokenStart;
            int tokenLen;
            TokenType currentTokenType;

            Token EmitToken()
            {
                var value = currentTokenType switch
                {
                    TokenType.Identifier => text.Substring(tokenStart, tokenLen),
                    TokenType.Literal => (object?)decimal.Parse(text.Substring(tokenStart, tokenLen), CultureInfo.InvariantCulture),
                    _ => null
                };
                return new Token(currentTokenType, value, tokenStart, tokenLen, saveText ? text : null);
            }

            Token NewToken(TokenType type, ref int i, int len = 1, object? value = null)
            {
                var tok = new Token(type, value, i, len, saveText ? text : null);
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
                    while (i < text.Length && (IsIdentifierChar(text[i]) || IsNumberChar(text[i])))
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
                    while (i < text.Length && (IsNumberChar(text[i]) || text[i] == '.'))
                    {
                        i++;
                        tokenLen++;
                    }
                    yield return EmitToken();
                }
                else if (text[i] == '"')
                {
                    tokenStart = i++;
                    tokenLen = 1;
                    var builder = new StringBuilder();
                    bool escaped = false;
                    while (i < text.Length && (escaped || text[i] != '"'))
                    {
                        if (!escaped && text[i] == '\\')
                            escaped = true;
                        else
                        {
                            builder.Append(text[i]);
                            escaped = false;
                        }

                        i++;
                        tokenLen++;
                    }
                    if (i >= text.Length || text[i++] != '"')
                    {
                        yield return new Token(TokenType.Error, "End of input while in string", tokenStart, tokenLen, saveText ? text : null);
                    }
                    else
                    {
                        tokenLen++;
                        yield return new Token(TokenType.String, builder.ToString(), tokenStart, tokenLen, saveText ? text : null);
                    }
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
                        case ',':
                            yield return NewToken(TokenType.Comma, ref i);
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
                        case '|':
                            yield return NewToken(TokenType.Or, ref i);
                            break;
                        case '~':
                            if (text[i + 1] == '&')
                                yield return NewToken(TokenType.NAnd, ref i, 2);
                            else if (text[i + 1] == '|')
                                yield return NewToken(TokenType.NOr, ref i, 2);
                            else if (text[i + 1] == '^')
                                yield return NewToken(TokenType.XNor, ref i, 2);
                            else if (text[i + 1] == '=')
                                yield return NewToken(TokenType.Inequals, ref i, 2);
                            else
                                yield return NewToken(TokenType.Not, ref i);
                            break;
                        case '!':
                            yield return NewToken(TokenType.Factorial, ref i);
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
                        case '\'':
                            yield return NewToken(TokenType.Prime, ref i);
                            break;
                        case ';':
                            yield return NewToken(TokenType.Semicolon, ref i);
                            break;
                        case '.':
                            yield return NewToken(TokenType.Period, ref i);
                            break;
                        default:
                            yield return NewToken(TokenType.Error, ref i, value: "Unexpected character");
                            break;
                    }
                }
            }
        }
    }
}
