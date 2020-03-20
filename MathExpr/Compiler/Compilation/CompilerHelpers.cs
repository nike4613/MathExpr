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
    /// <summary>
    /// Helper functions for compilation.
    /// </summary>
    public static class CompilerHelpers
    {
        private static void Assert(bool cond, string message, string arg)
        {
            if (!cond) throw new ArgumentException(message, arg);
        }

        /// <summary>
        /// Coerces the argument to a boolean, by effectively doing <c>arg != 0</c>.
        /// </summary>
        /// <param name="arg">the argument to coerce to a boolean type</param>
        /// <returns>an <see cref="Expression"/> coerced to a booelan</returns>
        public static Expression AsBoolean(Expression arg)
        {
            if (arg.Type == typeof(bool)) return arg;
            return Expression.NotEqual(arg, ConstantOfType(arg.Type, 0));
        }
        /// <summary>
        /// Coerces a boolean expression to a numeric expression, optionally as an inverse.
        /// </summary>
        /// <remarks>
        /// For the purposes of MathExpr, a value of zero is considered 'falsey', and anything nonzero
        /// is 'truthy'.
        /// </remarks>
        /// <param name="arg">the boolean expression to convert</param>
        /// <param name="type">the type to convert it to</param>
        /// <param name="inverse">whether or not to reat it as an inverse</param>
        /// <returns><paramref name="arg"/> as a numeric boolean.</returns>
        public static Expression BoolToNumBool(Expression arg, Type type, bool inverse = false)
        {
            Assert(arg.Type == typeof(bool), "Expression type must be boolean", nameof(arg));
            if (type == arg.Type) return arg;
            return Expression.Condition(arg,
                    ConstantOfType(type, inverse ? 0 : 1),
                    ConstantOfType(type, inverse ? 1 : 0));
        }
        /// <summary>
        /// Coerces a numeric argument to either 1 or 0 representing <see langword="true"/> or <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// This effectively calls <c><see cref="BoolToNumBool"/>(<see cref="AsBoolean"/>(<paramref name="arg"/>)</c>.
        /// </remarks>
        /// <param name="arg">the value to coerce to a numeric boolean value</param>
        /// <param name="inverse">whether or not to invert the result</param>
        /// <returns>the coerced value</returns>
        public static Expression CoerceNumBoolean(Expression arg, bool inverse = false)
            => BoolToNumBool(AsBoolean(arg), arg.Type, inverse);
        /// <summary>
        /// Emits a constant of the given type, if at all possible.
        /// </summary>
        /// <remarks>
        /// First this tries to use <see cref="Convert.ChangeType(object?, Type)"/> and embed the result.
        /// If that fails, it then embeds the passed value, and invokes <see cref="ConvertToType(Expression, Type)"/>
        /// on it.
        /// </remarks>
        /// <param name="type">the type of the constant to emit</param>
        /// <param name="val">the value to embed</param>
        /// <returns>a constant expression of type <paramref name="type"/></returns>
        public static Expression ConstantOfType(Type type, object? val)
        {
            try
            {
                return Expression.Constant(Convert.ChangeType(val, type), type);
            }
            catch (InvalidCastException)
            {
                // fallback to runtime conversion if possible
                return ConvertToType(Expression.Constant(val), type);
            }
        }
        /// <summary>
        /// Attempts to convert an expression to a particular type, using the shortest implicit and explicit conversion
        /// path between them.
        /// </summary>
        /// <remarks>
        /// Internally, this calls <see cref="FindConversionPathTo(Type, Type)"/> to determine the shortest path from
        /// one type to another, then uses <see cref="Expression.Convert(Expression, Type)"/> in sequence to convert the
        /// value.
        /// </remarks>
        /// <param name="expr">the expression to convert</param>
        /// <param name="type">the type to convert it to</param>
        /// <returns>the value after having gone through conversions</returns>
        public static Expression ConvertToType(Expression expr, Type type)
        {
            var path = FindConversionPathTo(expr.Type, type);
            if (path == null) throw new InvalidOperationException($"No conversion path exists from '{expr.Type}' to '{type}'");
            return path.Aggregate(expr, Expression.Convert);
        }

        /// <summary>
        /// Checks if the provided type is a primitive integral type.
        /// </summary>
        /// <param name="ty">the type to check</param>
        /// <returns><see langword="true"/> if <paramref name="ty"/> is a primitive integral type, <see langword="false"/>
        /// otherwise</returns>
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
        /// <summary>
        /// Checks if the provided type is a built-in floating point type.
        /// </summary>
        /// <param name="ty">the type to check</param>
        /// <returns><see langword="true"/> if <paramref name="ty"/> is <see cref="float"/>, <see cref="double"/>, or
        /// <see cref="decimal"/>, and <see langword="false"/> otherwise</returns>
        public static bool IsFloating(Type ty)
            => ty == typeof(float)
            || ty == typeof(double)
            || ty == typeof(decimal);
        /// <summary>
        /// Checks if the provided type is a primitive signed integer type.
        /// </summary>
        /// <param name="ty">the type to check</param>
        /// <returns><see langword="true"/> if <paramref name="ty"/> is a primitive signed integer type, 
        /// <see langword="false"/> otherwise</returns>
        public static bool IsSigned(Type ty)
            => ty.IsPrimitive
            && (ty == typeof(long)
            || ty == typeof(int)
            || ty == typeof(short)
            || ty == typeof(sbyte));

        /// <summary>
        /// Checks if there exists a conversion path from any type to any other type.
        /// </summary>
        /// <param name="from">the type to start from</param>
        /// <param name="to">the type to end at</param>
        /// <returns><see langword="true"/> if there is a conversion path, <see langword="false"/> otherwise.</returns>
        public static bool HasConversionPathTo(Type from, Type to) => FindConversionPathTo(from, to) != null;

        const BindingFlags OperatorFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static readonly Dictionary<Type, MethodInfo[]> ConversionOperatorCache = new Dictionary<Type, MethodInfo[]>();
        private static MethodInfo[] GetConversionOperators(Type ty)
        {
            if (!ConversionOperatorCache.TryGetValue(ty, out var values))
                ConversionOperatorCache.Add(ty, 
                    values = ty.GetMethods(OperatorFlags)
                               .Where(m => m.Name == "op_Implicit" || m.Name == "op_Explicit")
                               .ToArray());
            return values;
        }

        private static readonly Dictionary<(Type from, Type to), Type[]?> ConversionPathCache = new Dictionary<(Type from, Type to), Type[]?>();
        /// <summary>
        /// Attempts to find a conversion path from any type to any other type.
        /// </summary>
        /// <remarks>
        /// This function is cached, so only the first call with any given pair of types will run a full resolution.
        /// </remarks>
        /// <param name="from">the type to start from</param>
        /// <param name="to">the type to end at</param>
        /// <returns>an enumerable of types to cast to in order, excluding the starting type while including the ending type,
        /// or <see langword="null"/> if there is no such path</returns>
        public static IEnumerable<Type>? FindConversionPathTo(Type from, Type to)
        {
            if (!ConversionPathCache.TryGetValue((from, to), out var path))
                ConversionPathCache.Add((from, to), path = FindConversionPathToInternal(from, to)?.ToArray());
            return path;
        }
        private static IEnumerable<Type>? FindConversionPathToInternal(Type from, Type to)
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
        /// <summary>
        /// Attempts to estimate the size and precision of a given type.
        /// </summary>
        /// <remarks>
        /// For all of the types that there is a unique <see cref="TypeCode"/> for, this returns <c>sizeof(type)</c>.
        /// For both <see cref="float"/> and <see cref="double"/>, that size is incremented by 1 to indicate higher precision
        /// than the integers of the same size.
        /// 
        /// For all other value types, this returns the result of <see cref="Marshal.SizeOf{T}()"/> using <paramref name="type"/>
        /// as the type parameter.
        /// 
        /// For all reference types, this returns <see cref="int.MaxValue"/>.
        /// </remarks>
        /// <param name="type">the type to estimate the size and precision of</param>
        /// <returns>the estimated size</returns>
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
