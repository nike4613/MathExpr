using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace MathExpr.Compiler.Compilation
{
    public static class CompilerHelpers
    {
        private static void Assert(bool cond, string message, string arg)
        {
            if (cond) throw new ArgumentException(message, arg);
        }

        public static Expression AsBoolean(Expression arg)
        {
            if (arg.Type == typeof(bool)) return arg;
            return Expression.NotEqual(arg, ConstantOfType(arg.Type, 0));
        }
        public static Expression BoolToNumBool(Expression arg, Type type, bool inverse = false)
        {
            Assert(arg.Type == typeof(bool), "Expression type must be boolean", nameof(arg));
            if (type == arg.Type) return arg;
            return Expression.Condition(arg,
                    ConstantOfType(type, inverse ? 0 : 1),
                    ConstantOfType(type, inverse ? 1 : 0));
        }
        public static Expression CoerceNumBoolean(Expression arg, bool inverse = false)
            => BoolToNumBool(AsBoolean(arg), arg.Type, inverse);

        public static Expression ConstantOfType(Type type, object? val)
        {
            try
            {
                return Expression.Constant(Convert.ChangeType(val, type), type);
            }
            catch (InvalidCastException)
            {
                // fallback to runtime conversion if possible
                return Expression.Convert(Expression.Constant(val), type);
            }
        }
    }
}
