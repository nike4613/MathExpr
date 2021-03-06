﻿using MathExpr.Compiler;
using MathExpr.Compiler.Compilation;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class EasyInterfaceTests
    {
        [Theory]
        [InlineData("x", 1.2d, 1.2d)]
        [InlineData("customFunc'(x) = x; customFunc'(x)", 1.2d, 1.2d)]
        [InlineData("2*x", 1.5d, 3d)]
        [InlineData("2*x + 1", 1.5d, 4d)]
        public void CompileX(string expression, double arg, double expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<double, double>>(MathExpression.Parse(expression), optimize: false, "x");
            Assert.Equal(expect, del(arg));
        }

        [Theory]
        [InlineData("x", 1.2d, 0d, 1.2d)]
        [InlineData("2*x", 1.5d, 0d, 3d)]
        [InlineData("2*x + 1", 1.5d, 0d, 4d)]
        [InlineData("x*y", 1.2d, 0d, 0d)]
        [InlineData("2*x*y", 1.5d, 0d, 0d)]
        [InlineData("(2*x + 1)*y", 1.5d, 0d, 0d)]
        [InlineData("x*y", 1.2d, 2d, 2.4d)]
        [InlineData("2*x*y", 1.5d, 2d, 6d)]
        [InlineData("(2*x + 1)*y", 1.5d, 2d, 8d)]
        [InlineData("c'(x) = x*2+y; c'(x) + c'(y)", 1d, 2d, 10d)]
        public void CompileXY(string expression, double arg1, double arg2, double expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<double, double, double>>(MathExpression.Parse(expression), optimize: false, "x", "y");
            Assert.Equal(expect, del(arg1, arg2));
        }

        [Theory]
        [InlineData("obj", 0, 0)]
        [InlineData("obj", 1, 1)]
        [InlineData("obj", 2, 2)]
        [InlineData("~obj", 0, 42)]
        [InlineData("~obj", 1, 0)]
        [InlineData("(obj | 1) & ~obj", 1, 0)]
        [InlineData("(obj | 1) & ~obj", 0, 1)]
        public void CompileCustomType(string expression, int ctorArg, int expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<CustomType, int>>(MathExpression.Parse(expression), "obj");
            Assert.Equal(expect, del(new CustomType(ctorArg)));
        }

        [Theory]
        [InlineData("arg = \"teststr\"", "teststr", true)]
        [InlineData("arg = \"teststr\"", "teststr1", false)]
        [InlineData("arg = \"teststr\" | 1", "teststr", true)]
        [InlineData("arg = \"teststr\" | 1", "teststr1", true)]
        [InlineData("arg = \"teststr\" | 0", "teststr", true)]
        [InlineData("arg = \"teststr\" | 0", "teststr1", false)]
        public void CompileWithStringCompare(string expr, string arg, bool expect)
        {
            var del = ExpressionCompiler.Default.Compile<Func<string, bool>>(expr, "arg");
            Assert.Equal(expect, del(arg));
        }

        private class CustomType
        {
            public int Thing { get; }
            public CustomType(int t) => Thing = t;
            public static implicit operator CustomType(int i) => new CustomType(i);
            public static implicit operator int(CustomType t) => t.Thing;
            public static explicit operator CustomType(bool b) => new CustomType(b ? 42 : 0);
        }

        [Theory]
        [InlineData("customFunc'(x)")]
        [InlineData("defaultFunc(x)")]
        [InlineData("defaultFunc(x, y)")]
        [InlineData("sin(x, y)")]
        [InlineData("sin(z)")]
        public void CompileError(string expression)
        {
            Assert.Throws<CompilationException>(() =>
            {
                try
                {
                    ExpressionCompiler.Default.Compile<Func<double, double, double>>(MathExpression.Parse(expression), optimize: false, "x", "y");
                }
                catch (Exception e)
                {
                    _ = e.ToString();
                    throw;
                }
            });
        }
    }
}
