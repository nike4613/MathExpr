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
                    : throw new SyntaxException(t, t.AsString!)).AsLookahead(1); // should be able to get away with just 1 token of lookahead

            return new ExpressionParser(tokenStream).Read();
        }

        private readonly LookaheadEnumerable<Token> tokens;
        private ExpressionParser(LookaheadEnumerable<Token> toks)
            => tokens = toks;

        private bool TryConsumeToken(TokenType type, out Token token)
            => tokens.TryPeek(out token) && token.Type == type && tokens.TryNext(out token);

        private MathExpression Read()
        {
            var expr = ReadRoot();
            if (tokens.HasNext && tokens.TryNext(out var tok))
                throw new SyntaxException(tok, "Unexpected trailing token(s)");
            return expr;
        }

        private MathExpression ReadRoot()
            => ReadLogicExpr();

        private MathExpression ReadLogicExpr()
        {
            var left = ReadCompareExpr();
            while (TryConsumeToken(TokenType.And, out var tok) || TryConsumeToken(TokenType.NAnd, out tok)
                || TryConsumeToken(TokenType.Or, out tok) || TryConsumeToken(TokenType.NOr, out tok)
                || TryConsumeToken(TokenType.Xor, out tok) || TryConsumeToken(TokenType.XNor, out tok))
            {
                var right = ReadCompareExpr();
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.And => BinaryExpression.ExpressionType.And,
                    TokenType.NAnd => BinaryExpression.ExpressionType.NAnd,
                    TokenType.Or => BinaryExpression.ExpressionType.Or,
                    TokenType.NOr => BinaryExpression.ExpressionType.NOr,
                    TokenType.Xor => BinaryExpression.ExpressionType.Xor,
                    TokenType.XNor => BinaryExpression.ExpressionType.XNor,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, right);
            }
            return left;
        }

        private MathExpression ReadCompareExpr()
        {
            var left = ReadAddSubExpr();
            while (TryConsumeToken(TokenType.Equals, out var tok) || TryConsumeToken(TokenType.Inequals, out tok)
                || TryConsumeToken(TokenType.Less, out tok) || TryConsumeToken(TokenType.Greater, out tok)
                || TryConsumeToken(TokenType.LessEq, out tok) || TryConsumeToken(TokenType.GreaterEq, out tok))
            {
                var right = ReadAddSubExpr();
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Equals => BinaryExpression.ExpressionType.Equals,
                    TokenType.Inequals => BinaryExpression.ExpressionType.Inequals,
                    TokenType.Less => BinaryExpression.ExpressionType.Less,
                    TokenType.Greater => BinaryExpression.ExpressionType.Greater,
                    TokenType.LessEq => BinaryExpression.ExpressionType.LessEq,
                    TokenType.GreaterEq => BinaryExpression.ExpressionType.GreaterEq,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, right);
            }
            return left;
        }

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
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, right);
            }
            return left;
        }

        private MathExpression ReadMulDivExpr()
        {
            var left = ReadExponentExpr();
            while (TryConsumeToken(TokenType.Star, out var tok) || TryConsumeToken(TokenType.Slash, out tok)
                || TryConsumeToken(TokenType.Percent, out tok))
            {
                var right = ReadExponentExpr();
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Star => BinaryExpression.ExpressionType.Multiply,
                    TokenType.Slash => BinaryExpression.ExpressionType.Divide,
                    TokenType.Percent => BinaryExpression.ExpressionType.Modulo,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
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
                return new LiteralExpression(tok.AsDecimal!.Value);
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
