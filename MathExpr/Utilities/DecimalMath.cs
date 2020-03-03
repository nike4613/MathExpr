using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MathExpr.Utilities
{
    public static class DecimalMath
    {
        // x^n can be represented as sigma(v=0 -> inf, (n^v * log(x)^v) / v!)
        // ln(x) around 1 is sigma(n=1 -> inf, ((-1)^(n + 1) * (x - 1)^n)/n)
        private const MethodImplOptions AggressiveOptimization =
#if NETCOREAPP3_0
            MethodImplOptions.AggressiveOptimization;
#else
            (MethodImplOptions)0;
#endif

        [MethodImpl(AggressiveOptimization)]
        public static decimal Pow(decimal bas, decimal exponent, int iters = 6)
        {
            if (exponent == 0) return 1;
            if (exponent < 0) return 1m / Pow(bas, -exponent);
            var trunc = decimal.Truncate(exponent);
            if (exponent == trunc)
                return IntPow(bas, trunc);

            var center = decimal.Round(exponent);
            var centerC = IntPow(bas, decimal.Truncate(center));

            var logVal = Ln(bas);
            var logPow = 1m;
            var nPow = 1m;
            var vFac = 1m;
            var sum = 0m;
            for (int v = 0; v < iters; v++)
            {
                sum += centerC * nPow * logPow / vFac;
                nPow *= exponent - center;
                logPow *= logVal;
                vFac *= v + 1; // because this is now factorial for the *next* iteration
            }
            return sum;
        }

        [MethodImpl(AggressiveOptimization)]
        private static decimal IntPow(decimal bas, decimal exp)
        { // assumes exp is integer
            if (exp == 0) return 1m;
            if (exp == 1) return bas;
            if (exp == 2) return bas * bas;
            if (exp == 3) return bas * bas * bas;

            var bits = decimal.GetBits(exp); // lo mid hi flags

            var prod = 1m;

            uint dblCount;
            do
            {
                dblCount = Log2(bits);
                var idx = dblCount / 32;
                var valat = (uint)bits[idx];
                bits[idx] = (int)(valat ^ (1 << (int)dblCount));

                var lprod = bas;
                for (uint i = 0; i < dblCount; i++)
                    lprod *= lprod;
                prod *= lprod;
            } while (dblCount > 0 && (bits[0] > 0 || bits[0] > 0 || bits[0] > 0));

            return prod;
        }

        public const decimal Ln2 = 0.693147180559945309417232122m;

        /// <summary>
        /// The natural logarithm, approxamated by the Taylor series centered around 1.
        /// </summary>
        /// <param name="arg">the argument to <c>ln(x)</c></param>
        /// <param name="iters">the number of terms to use</param>
        /// <returns>the approxamate value of <c>ln(x)</c></returns>
        public static decimal Ln(decimal arg)
        {
            if (arg <= 0)
                throw new OverflowException("Ln not defined below 0");
            if (arg == 1) return 0;
            if (arg == 2) return Ln2;

            if (arg < 1)
                throw new NotImplementedException("approxamations for <0");

            // implementation based on https://stackoverflow.com/a/44232045

            var trunc = decimal.Truncate(arg);
            var truncBits = decimal.GetBits(trunc);

            var log = Log2(truncBits);
            var x = arg / HighBit(truncBits);

            #region Approxamation Polynomial
            // max error 1.1688695634449*10^(-14)
            /*
             * with(numapprox);
             * Digits := 27;
             * minimax(ln(x), x = 1 .. 2, 16, 1, 'maxerror');
             */
            var res = -3.03583533182214479766691286m +
                (11.3112680075496515844324792m +
                    (-29.8736012675475577430786512m +
                        (65.2083453445743710801323715m +
                            (-111.097825334293759973560852m +
                                (148.534071386071268988190931m +
                                    (-157.424462890917507884556818m +
                                        (133.225172268984311761239366m +
                                            (-90.2996769820337481793311886m +
                                                (48.9451340222356623913845767m +
                                                    (-21.0754655362975101325579042m +
                                                        (7.11671592394798144753402649m +
                                                            (-1.84432558485438263660851294m +
                                                                (0.354147129830249156762711239m +
                                                                    (-0.0474713112761932027336491138m +
                                                                        (0.00396558631788738252056417950m -
                                                                            0.000155430468567553406971315579m
                                                                            * x)
                                                                        * x)
                                                                    * x)
                                                                * x)
                                                            * x)
                                                        * x)
                                                    * x)
                                                * x)
                                            * x)
                                        * x)
                                    * x)
                                * x)
                            * x)
                        * x)
                    * x)
                * x;
            #endregion
            res += log * Ln2;

            return res;
        }

        /// <summary>
        /// Factorial. Typically notated <c>x!</c>.
        /// </summary>
        /// <remarks>
        /// Classically, factorial is not defined for non-integers, nor for values less than zero.
        /// However, there exists a gamma function such that <c>x! = gamma(x + 1)</c> and can be
        /// used as an extension. This implementation automatically forwards to <see cref="Gamma(decimal)"/>
        /// for all inputs that are not positive integers.
        /// </remarks>
        /// <param name="val">the argument to the factorial function</param>
        /// <returns><c>x!</c></returns>
        /// <seealso cref="Gamma(decimal)"/>
        [MethodImpl(AggressiveOptimization)]
        public static decimal Factorial(decimal val)
        {
            if (val == 0) return 1;
            if (val != decimal.Floor(val) || val < 0) // it is not an integral value
                return Gamma(val + 1);

            return IntFactorial(decimal.Truncate(val));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | AggressiveOptimization)]
        private static decimal IntFactorial(decimal val)
        { // assumes arg is integer
            if (val == 0) return 1;
            var prod = val;
            while (--val > 0)
                prod *= val;
            return prod;
        }

        /// <summary>
        /// The gamma factorial, an extension of factorial.
        /// </summary>
        /// <remarks>
        /// This function is wierd AF, so I just haven't implemented it.
        /// </remarks>
        /// <param name="val">the argument to the gamma function</param>
        /// <returns><c>gamma(x)</c></returns>
        /// <seealso cref="Factorial(decimal)"/>
        public static decimal Gamma(decimal val)
        {
            throw new NotImplementedException("The gamma function is too complicated for me fam");
        }

#if NETCOREAPP3_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Log2(uint n)
            => System.Runtime.Intrinsics.X86.Lzcnt.IsSupported
                ? 31 - System.Runtime.Intrinsics.X86.Lzcnt.LeadingZeroCount(n) // 31 is bitwidth of uint - 1
                : Log2_CS(n);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint HighBit(uint n)
            => System.Runtime.Intrinsics.X86.Lzcnt.IsSupported
                ? 0x80000000 >> (int)System.Runtime.Intrinsics.X86.Lzcnt.LeadingZeroCount(n)
                : HighBit_CS(n);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CountBits(uint n)
            => System.Runtime.Intrinsics.X86.Popcnt.IsSupported
               ? System.Runtime.Intrinsics.X86.Popcnt.PopCount(n)
               : CountBits_CS(n);
#else
        // no intrinsics to speak of, forward to CS impl
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Log2(uint n) => Log2_CS(n);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint HighBit(uint n) => HighBit_CS(n);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CountBits(uint n) => CountBits_CS(n);
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining | AggressiveOptimization)]
        private static uint Log2(int[] din)
        {
            if (din[2] != 0) return Log2((uint)din[2]) + 64;
            if (din[1] != 0) return Log2((uint)din[1]) + 32;
            return Log2((uint)din[0]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | AggressiveOptimization)]
        private static decimal HighBit(int[] din)
        {
            if (din[2] != 0) return new decimal(0, 0, (int)HighBit((uint)din[2]), din[3] < 0, 0);
            if (din[1] != 0) return new decimal(0, (int)HighBit((uint)din[1]), 0, din[3] < 0, 0);
            return new decimal((int)HighBit((uint)din[0]), 0, 0, din[3] < 0, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Log2_CS(uint n)
            => CountBits(HighBit(n) - 1);
        [MethodImpl(AggressiveOptimization)]
        private static uint HighBit_CS(uint n)
        {
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n ^ (n >> 1);
        }
        // source: http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
        [MethodImpl(AggressiveOptimization)]
        private static uint CountBits_CS(uint n)
        {
            n -= (n >> 1) & 0x55555555;
            n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
            return ((n + (n >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
        }
    }
}
