﻿using MathExpr.Compiler.Optimization.Settings;
using MathExpr.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MathExpr.Compiler.Compilation.Settings
{
    public delegate Expression TypedFactorialCompiler(Expression argument);
    public interface ICompileToLinqExpressionSettings : 
        IDomainRestrictionSettings,
        IBuiltinFunctionCompilerSettings<ICompileToLinqExpressionSettings> // TODO: refactor stuff a bit so that this isn't actually necessary
    {
        Type ExpectReturn { get; }
        IDictionary<VariableExpression, ParameterExpression> ParameterMap { get; }

        
        IDictionary<Type, TypedFactorialCompiler> TypedFactorialCompilers { get; }
        IList<ISpecialBinaryOperationCompiler> PowerCompilers { get; }
    }
}