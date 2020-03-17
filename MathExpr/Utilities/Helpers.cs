using System;
using System.Collections.Generic;
using System.Text;

namespace MathExpr.Utilities
{
    public static class Helpers
    {
        public static ulong IntegerFactorial(ulong val)
        {
            if (val == 0) return 1;
            var prod = val;
            while (--val > 0)
                prod *= val;
            return prod;
        }
    }
}
