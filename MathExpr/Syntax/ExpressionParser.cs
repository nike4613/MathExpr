using MathExpr.Compiler.Compilation.Builtins;
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
        public static MathExpression ParseRoot(string s, bool saveText = true)
        {
            var tokenStream = Tokenizer.Tokenize(s, saveText)
                .Where(t => t.Type != TokenType.LineComment && t.Type != TokenType.BlockComment) // ignore comment tokens
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
            if (tokens.HasNext && tokens.TryNext(out var tok) && tok.Type != TokenType.EndOfInput)
                throw new SyntaxException(tok, "Unexpected trailing token(s)");
            return expr;
        }

        private MathExpression ReadRoot()
            => ReadDefinition();

        private MathExpression ReadDefinition()
        {
            MathExpression left;
            try
            { // This is a sync point; if we encounter an error, we can resume parsing after the semicolon 
                left = ReadLogicExpr();
            }
            catch (SyntaxException e)
            {
                // advance until we get a token of type Semicolon
                while (tokens.TryPeek(out var tok) && tok.Type != TokenType.Semicolon)
                    _ = tokens.Next();

                try
                {
                    // try to parse the remainder of the input using a dummy function definition
                    _ = Consume(this, new BinaryExpression(
                        new FunctionExpression("<>__NonexisentFunc", new List<MathExpression>(), true),
                        new LiteralExpression(0),
                        BinaryExpression.ExpressionType.Equals
                    ));
                }
                catch (SyntaxException e2)
                {
                    throw new AggregateException(e, e2);
                }
                catch (AggregateException e2)
                {
                    throw new AggregateException(Helpers.Single(e).Concat(e2.InnerExceptions));
                }

                throw;
            }

            static MathExpression Consume(in ExpressionParser _this, MathExpression left)
            {
                if (_this.TryConsumeToken(TokenType.Semicolon, out var tok))
                {
                    if (!(left is BinaryExpression bleft))
                        throw new SyntaxException(left.Token, $"Left hand of semicolon must be a custom function definition");

                    var right = _this.ReadDefinition();

                    try
                    {
                        return new CustomDefinitionExpression(bleft, right).WithToken(left.Token);
                    }
                    catch (ArgumentException e)
                    {
                        throw new SyntaxException(tok, e.Message);
                    }
                }
                return left;
            }

            return Consume(this, left);
        }

        private MathExpression ReadLogicExpr()
        {
            var left = ReadCompareExpr();
            while (TryConsumeToken(TokenType.And, out var tok) || TryConsumeToken(TokenType.NAnd, out tok)
                || TryConsumeToken(TokenType.Or, out tok) || TryConsumeToken(TokenType.NOr, out tok)
                || TryConsumeToken(TokenType.Xor, out tok) || TryConsumeToken(TokenType.XNor, out tok))
            {
                left = new BinaryExpression(left, ReadCompareExpr(), tok.Type switch
                {
                    TokenType.And => BinaryExpression.ExpressionType.And,
                    TokenType.NAnd => BinaryExpression.ExpressionType.NAnd,
                    TokenType.Or => BinaryExpression.ExpressionType.Or,
                    TokenType.NOr => BinaryExpression.ExpressionType.NOr,
                    TokenType.Xor => BinaryExpression.ExpressionType.Xor,
                    TokenType.XNor => BinaryExpression.ExpressionType.XNor,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }).WithToken(tok);
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
                left = new BinaryExpression(left, ReadAddSubExpr(), tok.Type switch
                {
                    TokenType.Equals => BinaryExpression.ExpressionType.Equals,
                    TokenType.Inequals => BinaryExpression.ExpressionType.Inequals,
                    TokenType.Less => BinaryExpression.ExpressionType.Less,
                    TokenType.Greater => BinaryExpression.ExpressionType.Greater,
                    TokenType.LessEq => BinaryExpression.ExpressionType.LessEq,
                    TokenType.GreaterEq => BinaryExpression.ExpressionType.GreaterEq,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }).WithToken(tok);
            }
            return left;
        }

        private MathExpression ReadAddSubExpr()
        {
            var left = ReadMulDivExpr();
            while (TryConsumeToken(TokenType.Plus, out var tok) || TryConsumeToken(TokenType.Minus, out tok))
            {
                left = new BinaryExpression(left, ReadMulDivExpr(), tok.Type switch
                {
                    TokenType.Plus => BinaryExpression.ExpressionType.Add,
                    TokenType.Minus => BinaryExpression.ExpressionType.Subtract,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }).WithToken(tok);
            }
            return left;
        }

        private MathExpression ReadMulDivExpr()
        {
            var left = ReadExponentExpr();
            while (TryConsumeToken(TokenType.Star, out var tok) || TryConsumeToken(TokenType.Slash, out tok)
                || TryConsumeToken(TokenType.Percent, out tok))
            {
                left = new BinaryExpression(left, ReadExponentExpr(), tok.Type switch
                {
                    TokenType.Star => BinaryExpression.ExpressionType.Multiply,
                    TokenType.Slash => BinaryExpression.ExpressionType.Divide,
                    TokenType.Percent => BinaryExpression.ExpressionType.Modulo,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }).WithToken(tok);
            }
            return left;
        }

        private MathExpression ReadExponentExpr()
        {
            var left = ReadNegateNotExpr();
            while (TryConsumeToken(TokenType.Exponent, out var tok))
            {
                var right = ReadNegateNotExpr();
                left = new BinaryExpression(left, right, BinaryExpression.ExpressionType.Power).WithToken(tok);
            }
            return left;
        }

        private MathExpression ReadNegateNotExpr()
        {
            if (TryConsumeToken(TokenType.Not, out var tok) || TryConsumeToken(TokenType.Minus, out tok))
                return new UnaryExpression(tok.Type switch
                {
                    TokenType.Not => UnaryExpression.ExpressionType.Not,
                    TokenType.Minus => UnaryExpression.ExpressionType.Negate,
                    _ => throw new SyntaxException(tok, "Unexpected token type")
                }, ReadFactorialExpr()).WithToken(tok);
            else return ReadFactorialExpr();
        }

        private MathExpression ReadFactorialExpr()
        {
            var arg = ReadMemberExpr();
            while (TryConsumeToken(TokenType.Factorial, out var tok))
                arg = new UnaryExpression(UnaryExpression.ExpressionType.Factorial, arg).WithToken(tok);
            return arg;
        }

        private MathExpression ReadMemberExpr()
        {
            var left = ReadParenExpr();
            while (TryConsumeToken(TokenType.Period, out var dotTok))
            {
                if (!TryConsumeToken(TokenType.Identifier, out var tok))
                    throw new SyntaxException(tok, "Expected Identifier");
                left = new MemberExpression(left, tok.AsString!).WithToken(dotTok);
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
                return new LiteralExpression(tok.AsDecimal!.Value).WithToken(tok);
            else if (TryConsumeToken(TokenType.String, out tok))
                return new StringExpression(tok.AsString!).WithToken(tok);
            else if (TryConsumeToken(TokenType.Identifier, out tok))
            {
                if (TryConsumeToken(TokenType.Prime, out _)) // a prime function
                    return new FunctionExpression(tok.AsString!, ReadCallParamList().ToList(), true).WithToken(tok);
                else if (CheckNextToken(TokenType.OpenParen, out _)) // a normal function
                    return new FunctionExpression(tok.AsString!, ReadCallParamList().ToList(), false).WithToken(tok);
                else return new VariableExpression(tok.AsString!).WithToken(tok);
            }
            else if (TryConsumeToken(TokenType.EndOfInput, out tok))
                throw new SyntaxException(tok, "Unexpected end of input");
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

    /// <summary>
    /// An exception thrown when the parser fails to parse the input for any reason.
    /// </summary>
    public class SyntaxException : Exception
    {
        /// <summary>
        /// The token that caused the error, if any.
        /// </summary>
        public Token? Token { get; }

        /// <summary>
        /// Constructs a new instance of the exception with a token and a message.
        /// </summary>
        /// <param name="token">the token that caused the error, if any</param>
        /// <param name="message">the message relating to the error</param>
        public SyntaxException(Token? token, string message) 
            : base(message + (token == null ? "" : $" at {token.Value.Position}:{token.Value.Length}"))
        {
            Token = token;
        }

        /// <inheritdoc/>
        public override string ToString()
            => (Token?.FormatTokenLocation() ?? "") + base.ToString();
    }
}
