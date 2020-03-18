using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using MathExpr.Utilities;
using System.Runtime.InteropServices;

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

        public static bool IsIntegral(Type ty)
            => ty.IsPrimitive
            && (ty == typeof(long)
            || ty == typeof(ulong)
            || ty == typeof(int)
            || ty == typeof(uint)
            || ty == typeof(short)
            || ty == typeof(ushort)
            || ty == typeof(byte)
            || ty == typeof(sbyte));
        public static bool IsFloating(Type ty)
            => ty == typeof(float)
            || ty == typeof(double)
            || ty == typeof(decimal);
        public static bool IsSigned(Type ty)
            => ty.IsPrimitive
            && (ty == typeof(long)
            || ty == typeof(int)
            || ty == typeof(short)
            || ty == typeof(sbyte));

        public static bool HasConversionPathTo(Type from, Type to) => FindConversionPathTo(from, to) != null;

        const BindingFlags OperatorFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static Dictionary<Type, MethodInfo[]> ConversionOperatorCache = new Dictionary<Type, MethodInfo[]>();
        private static MethodInfo[] GetConversionOperators(Type ty)
        {
            if (!ConversionOperatorCache.TryGetValue(ty, out var values))
                ConversionOperatorCache.Add(ty, 
                    values = ty.GetMethods(OperatorFlags)
                               .Where(m => m.Name == "op_Implicit" || m.Name == "op_Explicit")
                               .ToArray());
            return values;
        }
        public static IEnumerable<Type>? FindConversionPathTo(Type from, Type to)
        {
            if (from == to) return Enumerable.Empty<Type>();
            if (to.IsAssignableFrom(from)) return Enumerable.Empty<Type>();
            if (from.IsAssignableFrom(to)) return Helpers.Single(to);

            if (from.IsPrimitive && to.IsPrimitive)
            {
                if ((IsIntegral(from) || IsFloating(from))
                 && (IsIntegral(to) || IsFloating(to)))
                    return Helpers.Single(to);
            }

            var fromPath = GetConversionOperators(from).Where(m => m.GetParameters()[0].ParameterType == from)
                .Select(m => m.ReturnType == to
                                ? Helpers.Single(to)
                                : (FindConversionPathTo(m.ReturnType, to)?.Prepend(m.ReturnType) ?? Enumerable.Empty<Type>()))
                .Select(e => e.ToList())
                .Where(l => l.Any() && l.Last() == to)
                .OrderBy(l => l.Count)
                .FirstOrDefault();

            var toPath = GetConversionOperators(to).Where(m => m.ReturnType == to)
                .Select(m => 
                {
                    var paramType = m.GetParameters()[0].ParameterType;
                    return paramType == from
                                 ? Helpers.Single(to)
                                 : (FindConversionPathTo(from, paramType)?.Append(to) ?? Enumerable.Empty<Type>());
                })
                .Select(e => e.ToList())
                .Where(l => l.Any() && l.Last() == to)
                .OrderBy(l => l.Count)
                .FirstOrDefault();

            if (fromPath == null) return toPath;
            if (toPath == null) return fromPath;
            return fromPath.Count < toPath.Count ? fromPath : toPath;
        }

        private static readonly MethodInfo MarshalSizeOfMethod = Helpers.GetMethod<Action>(() => Marshal.SizeOf<int>())!.GetGenericMethodDefinition();
        public static int EstimateTypeSize(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return sizeof(bool);
                case TypeCode.Char: return sizeof(char);
                case TypeCode.Byte: return sizeof(byte);
                case TypeCode.SByte: return sizeof(sbyte);
                case TypeCode.Int16: return sizeof(short);
                case TypeCode.UInt16: return sizeof(ushort);
                case TypeCode.Int32: return sizeof(int);
                case TypeCode.UInt32: return sizeof(uint);
                case TypeCode.Int64: return sizeof(long);
                case TypeCode.UInt64: return sizeof(ulong);
                case TypeCode.Single: return sizeof(float) + 1; // floating points get +1 because of extra precision
                case TypeCode.Double: return sizeof(double) + 1;
                case TypeCode.Decimal: return sizeof(decimal); // decimal doesn't because it is much less special than other floating point types
                default:
                    if (!type.IsValueType) return int.MaxValue; // there isn't a good way to estimate reference type sizes, so we assume better
                    return (int)MarshalSizeOfMethod.MakeGenericMethod(type).Invoke(null, Array.Empty<object>())!;
            }
        }
    }
}
