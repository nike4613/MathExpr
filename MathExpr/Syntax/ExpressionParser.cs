using MathExpr.Utilities;
using System;
using System.Collections.Generic;
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

        private bool CheckNextToken(TokenType type, out Token token)
            => tokens.TryPeek(out token) && token.Type == type;
        private bool TryConsumeToken(TokenType type, out Token token)
            => CheckNextToken(type, out token) && tokens.TryNext(out token);

        private MathExpression Read()
        {
            var expr = ReadRoot();
            if (tokens.HasNext && tokens.TryNext(out var tok))
                throw new SyntaxException(tok, "Unexpected trailing token(s)");
            return expr;
        }

        private MathExpression ReadRoot()
            => ReadDefinition();

        private MathExpression ReadDefinition()
        {
            var left = ReadLogicExpr();
            if (TryConsumeToken(TokenType.Semicolon, out var tok))
            {
                var right = ReadDefinition();
                try
                {
                    return new CustomDefinitionExpression(left, right);
                }
                catch (ArgumentException e)
                {
                    throw new SyntaxException(tok, e.Message);
                }
            }
            return left;
        }

        private MathExpression ReadLogicExpr()
        {
            var left = ReadCompareExpr();
            while (TryConsumeToken(TokenType.And, out var tok) || TryConsumeToken(TokenType.NAnd, out tok)
                || TryConsumeToken(TokenType.Or, out tok) || TryConsumeToken(TokenType.NOr, out tok)
                || TryConsumeToken(TokenType.Xor, out tok) || TryConsumeToken(TokenType.XNor, out tok))
            {
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.And => BinaryExpression.ExpressionType.And,
                    TokenType.NAnd => BinaryExpression.ExpressionType.NAnd,
                    TokenType.Or => BinaryExpression.ExpressionType.Or,
                    TokenType.NOr => BinaryExpression.ExpressionType.NOr,
                    TokenType.Xor => BinaryExpression.ExpressionType.Xor,
                    TokenType.XNor => BinaryExpression.ExpressionType.XNor,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadCompareExpr());
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
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Equals => BinaryExpression.ExpressionType.Equals,
                    TokenType.Inequals => BinaryExpression.ExpressionType.Inequals,
                    TokenType.Less => BinaryExpression.ExpressionType.Less,
                    TokenType.Greater => BinaryExpression.ExpressionType.Greater,
                    TokenType.LessEq => BinaryExpression.ExpressionType.LessEq,
                    TokenType.GreaterEq => BinaryExpression.ExpressionType.GreaterEq,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadAddSubExpr());
            }
            return left;
        }

        private MathExpression ReadAddSubExpr()
        {
            var left = ReadMulDivExpr();
            while (TryConsumeToken(TokenType.Plus, out var tok) || TryConsumeToken(TokenType.Minus, out tok))
            {
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Plus => BinaryExpression.ExpressionType.Add,
                    TokenType.Minus => BinaryExpression.ExpressionType.Subtract,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadMulDivExpr());
            }
            return left;
        }

        private MathExpression ReadMulDivExpr()
        {
            var left = ReadExponentExpr();
            while (TryConsumeToken(TokenType.Star, out var tok) || TryConsumeToken(TokenType.Slash, out tok)
                || TryConsumeToken(TokenType.Percent, out tok))
            {
                left = new BinaryExpression(left, tok.Type switch
                {
                    TokenType.Star => BinaryExpression.ExpressionType.Multiply,
                    TokenType.Slash => BinaryExpression.ExpressionType.Divide,
                    TokenType.Percent => BinaryExpression.ExpressionType.Modulo,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadExponentExpr());
            }
            return left;
        }

        private MathExpression ReadExponentExpr()
        {
            var left = ReadNegateNotExpr();
            while (TryConsumeToken(TokenType.Exponent, out _))
            {
                var right = ReadNegateNotExpr();
                left = new BinaryExpression(left, BinaryExpression.ExpressionType.Power, right);
            }
            return left;
        }

        private MathExpression ReadNegateNotExpr()
        {
            if (TryConsumeToken(TokenType.Tilde, out var tok) || TryConsumeToken(TokenType.Minus, out tok))
                return new UnaryExpression(tok.Type switch
                {
                    TokenType.Tilde => UnaryExpression.ExpressionType.Not,
                    TokenType.Minus => UnaryExpression.ExpressionType.Negate,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadFactorialExpr());
            else return ReadFactorialExpr();
        }

        private MathExpression ReadFactorialExpr()
        {
            var arg = ReadMemberExpr();
            while (TryConsumeToken(TokenType.Bang, out _))
                arg = new UnaryExpression(UnaryExpression.ExpressionType.Factorial, arg);
            return arg;
        }

        private MathExpression ReadMemberExpr()
        {
            var left = ReadParenExpr();
            while (TryConsumeToken(TokenType.Period, out _))
            {
                if (!TryConsumeToken(TokenType.Identifier, out var tok))
                    throw new SyntaxException(tok, "Expected Identifier");
                left = new MemberExpression(left, tok.AsString!);
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
                if (TryConsumeToken(TokenType.Prime, out _)) // a prime function
                    return new FunctionExpression(tok.AsString!, ReadCallParamList().ToList(), true);
                else if (CheckNextToken(TokenType.OpenParen, out _)) // a normal function
                    return new FunctionExpression(tok.AsString!, ReadCallParamList().ToList(), false);
                else return new VariableExpression(tok.AsString!);
            }
            else
                throw new SyntaxException(tok, "Unexpected token");
        }

        private IEnumerable<MathExpression> ReadCallParamList()
        {
            if (!TryConsumeToken(TokenType.OpenParen, out var tok))
                throw new SyntaxException(tok, "Expected '(' to start function parameter list");
            if (TryConsumeToken(TokenType.CloseParen, out _))
                yield break;

            do yield return ReadRoot();
            while (TryConsumeToken(TokenType.Comma, out _));

            if (!TryConsumeToken(TokenType.CloseParen, out _))
                throw new SyntaxException(tok, "Expected ')' to end function parameter list");
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
