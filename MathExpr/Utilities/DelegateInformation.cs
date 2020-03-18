using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MathExpr.Utilities
{
    public static class DelegateInformation<TDelegate> 
        where TDelegate : Delegate
    {
        public static readonly Type ReturnType;
        public static readonly Type[] ParamTypes;

        static DelegateInformation()
        {
            var del = typeof(TDelegate);
            var invoke = del.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            ReturnType = invoke!.ReturnType;
            ParamTypes = invoke!.GetParameters().Select(p => p.ParameterType).ToArray();
        }
    }
}
