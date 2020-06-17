using MathExpr.Compiler.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MathExprTests
{
    public class ConversionPathTests
    {
        [Theory]
        [InlineData(typeof(decimal), typeof(int), new[] { typeof(int) })]
        [InlineData(typeof(int), typeof(decimal), new[] { typeof(decimal) })]
        [InlineData(typeof(int), typeof(long), new[] { typeof(long) })]
        [InlineData(typeof(A), typeof(C), new[] { typeof(B), typeof(C) })]
        [InlineData(typeof(A), typeof(D), new[] { typeof(B), typeof(D) })]
        [InlineData(typeof(C), typeof(A), new[] { typeof(D), typeof(A) })]
        [InlineData(typeof(A), typeof(E), new[] { typeof(B), typeof(D), typeof(E) })]
        [InlineData(typeof(E), typeof(A), null)]
        [InlineData(typeof(A), typeof(int), new[] { typeof(bool), typeof(int) })]
        [InlineData(typeof(int), typeof(A), new[] { typeof(bool), typeof(A) })]
        public void TestFindConversion(Type from, Type to, Type[]? path)
        {
            Assert.Equal(path, CompilerHelpers.FindConversionPathTo(from, to)?.Select(n => n.ToType));
        }

        private class A
        {
            public static implicit operator B(A _) => new B();
            public static implicit operator A(D _) => new A();

            public static implicit operator bool(A _) => true;
            public static implicit operator A(bool _) => new A();
        }
        private class B
        {
            public static implicit operator C(B _) => new C();
        }
        private class C
        {
            public static implicit operator D(C _) => new D();
        }
        private class D
        {
            public static implicit operator D(B _) => new D();
        }
        private class E
        { 
            public static implicit operator E(D _) => new E();
        }
    }
}
