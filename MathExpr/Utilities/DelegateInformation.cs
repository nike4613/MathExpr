using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MathExpr.Utilities
{
    /// <summary>
    /// A container class that contains information about the delegate type <typeparamref name="TDelegate"/>.
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    public static class DelegateInformation<TDelegate> 
        where TDelegate : Delegate
    {
        /// <summary>
        /// The return type of <typeparamref name="TDelegate"/>.
        /// </summary>
        public static readonly Type ReturnType;
        /// <summary>
        /// The parameter types of <typeparamref name="TDelegate"/>.
        /// </summary>
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
