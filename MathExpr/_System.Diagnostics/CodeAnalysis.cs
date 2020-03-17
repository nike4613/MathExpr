﻿using System;
using System.Collections.Generic;
using System.Text;

namespace System.Diagnostics.CodeAnalysis
{
#if NETSTANDARD2_0
    // Effectively the Microsoft implementation for when it doesn't exist for my convenience

    /// <summary>
    /// Specifies that when a method returns <see cref="ReturnValue"/>,
    /// the parameter may be <see langword="null"/> even if the corresponding type disallows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        /// <summary>
        /// Initializes the attribute with the specified return value condition.
        /// </summary>
        /// <param name="returnValue">The return value condition. If the method returns this
        /// value, the associated parameter may be null.</param>
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>
        /// Gets the return value condition.
        /// </summary>
        /// <value>The return value condition. If the method returns this value, the
        /// associated parameter may be null.</value>
        public bool ReturnValue { get; }
    }
#endif
}
