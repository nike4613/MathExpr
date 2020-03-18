using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using MathExpr.Utilities;

namespace MathExpr.Compiler.Compilation
{
    public static class CompilerHelpers
    {
        private static void Assert(bool cond, string message, string arg)
        {
            if (!cond) throw new ArgumentException(message, arg);
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
        
        public static Expression ConvertToType(Expression expr, Type type)
        {
            var path = FindConversionPathTo(expr.Type, type);
            if (path == null) throw new InvalidOperationException($"No conversion path exists from '{expr.Type}' to '{type}'");
            return path.Aggregate(expr, Expression.Convert);
        }

        public static bool HasConversionPathTo(Type from, Type to) => FindConversionPathTo(from, to) != null;
        public static IEnumerable<Type>? FindConversionPathTo(Type from, Type to)
        {
            if (from == to) return Enumerable.Empty<Type>();
            if (to.IsAssignableFrom(from)) return Enumerable.Empty<Type>();
            if (from.IsAssignableFrom(to)) return Helpers.Single(to);

            const BindingFlags OperatorFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var fromOps = from.GetMethods(OperatorFlags).Where(m => m.Name == "op_Implicit" || m.Name == "op_Explicit");

            var fromPath = fromOps.Where(m => m.GetParameters()[0].ParameterType == from)
                .Select(m => m.ReturnType == to
                                ? Helpers.Single(to)
                                : (FindConversionPathTo(m.ReturnType, to) ?? Enumerable.Empty<Type>())
                                    .Prepend(m.ReturnType))
                .Select(e => e.ToList())
                .Where(l => l.Last() == to).FirstOrDefault();

            var toOps = to.GetMethods(OperatorFlags).Where(m => m.Name == "op_Implicit" || m.Name == "op_Explicit");
            var toPath = toOps.Where(m => m.ReturnType == to)
                .Select(m => 
                {
                    var paramType = m.GetParameters()[0].ParameterType;
                    return paramType == from
                                 ? Helpers.Single(to)
                                 : (FindConversionPathTo(from, paramType) ?? Enumerable.Empty<Type>())
                                    .Append(to);
                })
                .Select(e => e.ToList())
                .Where(l => l.Last() == to).FirstOrDefault();

            if (fromPath == null) return toPath;
            if (toPath == null) return fromPath;
            return fromPath.Count < toPath.Count ? fromPath : toPath;
        }
    }
}
