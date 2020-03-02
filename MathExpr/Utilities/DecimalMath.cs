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
        
        public static decimal Pow(decimal bas, decimal exponent, int iters = 6, int logIters = 16)
        {
            if (exponent == 0) return 1;
            if (exponent < 0) return 1m / Pow(bas, -exponent);
            var trunc = decimal.Truncate(exponent);
            if (exponent == trunc)
                return IntPow(bas, trunc);

            var center = decimal.Round(exponent);
            var centerC = IntPow(bas, decimal.Truncate(center));

            var logVal = Ln(bas, logIters);
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
                dblCount = Log2((uint)bits[2]); // hi
                if (dblCount > 0 && dblCount != uint.MaxValue)
                {
                    dblCount += 64;
                    bits[2] = (int)((uint)bits[2] - HighBit((uint)bits[2]));
                }
                else
                {
                    dblCount = Log2((uint)bits[1]); // mid
                    if (dblCount > 0 && dblCount != uint.MaxValue)
                    {
                        dblCount += 32;
                        bits[1] = (int)((uint)bits[1] - HighBit((uint)bits[1]));
                    }
                    else
                    {
                        dblCount = Log2((uint)bits[0]); // lo
                        if (dblCount > 0 && dblCount != uint.MaxValue) bits[0] = (int)((uint)bits[0] - HighBit((uint)bits[0]));
                    }
                }

                var lprod = bas;
                for (uint i = 0; i < dblCount; i++)
                    lprod *= lprod;
                prod *= lprod;
            } while (dblCount > 0 && (bits[0] > 0 || bits[0] > 0 || bits[0] > 0));

            return prod;
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
                : HighBitM1_CS(n) + 1;
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
        private static uint HighBit(uint n) => HighBitM1_CS(n) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CountBits(uint n) => CountBits_CS(n);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Log2_CS(uint n)
            => CountBits(HighBitM1_CS(n));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint HighBitM1_CS(uint n)
        {
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n >> 1;
        }
        // source: http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CountBits_CS(uint n)
        {
            n -= (n >> 1) & 0x55555555;
            n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
            return ((n + (n >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
        }

        /// <summary>
        /// The natural logarithm, approxamated by the Taylor series centered around 1.
        /// </summary>
        /// <param name="arg">the argument to <c>ln(x)</c></param>
        /// <param name="iters">the number of terms to use</param>
        /// <returns>the approxamate value of <c>ln(x)</c></returns>
        public static decimal Ln(decimal arg, int iters = 1024)
        {
            if (arg == 1) return 0;

            // TODO: figure out how to center this thing elsewhere (possibly different algo for integer log)
            if (Math.Abs(1 - arg) > 1) 
                throw new OverflowException("Series diverges");
            var args1p = arg - 1;
            var pow = args1p;
            var sum = 0m;
            for (int n = 1; n <= iters; n++)
            {
                var val = pow / n;
                pow *= args1p;
                if (n % 2 == 1) sum += val;
                else sum -= val;
            }
            return sum;
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
        public static decimal Factorial(decimal val)
        {
            if (val == 0) return 1;
            if (val != decimal.Floor(val) || val < 0) // it is not an integral value
                return Gamma(val + 1);

            return IntFactorial(decimal.Truncate(val));
        }
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
    }
}
