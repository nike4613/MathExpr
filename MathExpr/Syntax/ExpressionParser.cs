using MathExpr.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MathExpr.Syntax
{
    internal struct ExpressionParser
    {
        public static MathExpression ParseRoot(string s)
        {
            var tokenStream = Tokenizer.Tokenize(s)
                .Select(t => t.Type != TokenType.Error 
                    ? t 
                    : throw new SyntaxException(t, t.AsString!)).AsLookahead();

            return new ExpressionParser(tokenStream).ReadAddSubExpr();
        }

        private readonly LookaheadEnumerable<Token> tokens;
        private ExpressionParser(LookaheadEnumerable<Token> toks)
            => tokens = toks;

        private bool TryConsumeToken(TokenType type, out Token token)
            => tokens.TryPeek(out token) && token.Type == type && tokens.TryNext(out token);

        private MathExpression ReadRoot()
            => ReadAddSubExpr();

        private MathExpression ReadAddSubExpr()
        {
            var left = ReadMulDivExpr();
            while (TryConsumeToken(TokenType.Plus, out var tok) || TryConsumeToken(TokenType.Minus, out tok))
            {
                var right = ReadMulDivExpr();
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Plus => BinaryExpression.ExpressionType.Add,
                    TokenType.Minus => BinaryExpression.ExpressionType.Subtract,
                    _ => throw new Exception($"Unexpected token type {tok.Type}")
                }, right);
            }
            return left;
        }

        private MathExpression ReadMulDivExpr()
        {
            var left = ReadExponentExpr();
            while (TryConsumeToken(TokenType.Star, out var tok) || TryConsumeToken(TokenType.Slash, out tok))
            {
                var right = ReadExponentExpr();
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Star => BinaryExpression.ExpressionType.Multiply,
                    TokenType.Slash => BinaryExpression.ExpressionType.Divide,
                    _ => throw new Exception($"Unexpected token type {tok.Type}")
                }, right);
            }
            return left;
        }

        private MathExpression ReadExponentExpr()
        {
            var left = ReadParenExpr();
            while (TryConsumeToken(TokenType.Exponent, out var tok))
            {
                var right = ReadParenExpr();
                left = new BinaryExpression(left, BinaryExpression.ExpressionType.Exponent, right);
            }
            return left;
        }

        private MathExpression ReadParenExpr()
        {
            if (TryConsumeToken(TokenType.OpenParen, out _))
            {
                var expr = ReadRoot();
                if (!TryConsumeToken(TokenType.CloseParen, out var tok))
                    throw new SyntaxException(tok, $"Expected ')'");
                else
                    return expr;
            }
            else return ReadVarFuncLitExpr();
        }

        private MathExpression ReadVarFuncLitExpr()
        {
            if (TryConsumeToken(TokenType.Literal, out var tok))
                return new LiteralExpression(tok.AsDouble!.Value);
            else if (TryConsumeToken(TokenType.Identifier, out tok))
            {
                // TODO: check for function call
                return new VariableExpression(tok.AsString!);
            }
            else
                throw new SyntaxException(tok, "Expected literal or identifier");
        }
    }

    public class SyntaxException : Exception
    {
        public Token Token { get; }

        public SyntaxException(Token token, string message) : base($"{message} at token {token} at {token.Position}:{token.Length}")
        {
            Token = token;
        }
    }
}
