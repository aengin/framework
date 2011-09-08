﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Utilities.ExpressionTrees;
using System.Linq.Expressions;
using System.Reflection;
using Signum.Utilities.Reflection;

namespace Signum.Engine.Linq
{
    /// <summary>
    /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
    /// </summary>
    public class MetaEvaluator : SimpleExpressionVisitor
    {
        public static Expression Clean(Expression expression)
        {
            Expression expand = ExpressionCleaner.Clean(expression, MetaEvaluator.PartialEval, false);
            Expression simplified = OverloadingSimplifier.Simplify(expand);
            return simplified;
        }

        HashSet<Expression> candidates;

        private MetaEvaluator() { }

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression exp)
        {
            return new MetaEvaluator { candidates = ExpressionNominator.Nominate(exp) }.Visit(exp);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            if (this.candidates.Contains(exp) && exp.NodeType != ExpressionType.Constant)
            {
                if (exp.Type.IsAssignableFrom(typeof(IQueryable<>)))
                    return ExpressionEvaluator.PartialEval(exp);

                return (ConstantExpression)miConstant.GetInvoker(exp.Type)();
            }
            return base.Visit(exp);
        }

        static GenericInvoker miConstant = GenericInvoker.Create(() => Constant<int>());
        static ConstantExpression Constant<T>()
        {
            return Expression.Constant(default(T), typeof(T)); 
        }
    }
}
