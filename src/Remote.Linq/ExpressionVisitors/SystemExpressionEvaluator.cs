﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.ExpressionVisitors
{
    using Aqua.TypeExtensions;
    using Aqua.TypeSystem;
    using Remote.Linq.DynamicQuery;
    using Remote.Linq.ExpressionExecution;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Enables the partial evalutation of queries.
    /// From http://msdn.microsoft.com/en-us/library/bb546158.aspx.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SystemExpressionEvaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="canBeEvaluatedLocally">A function that decides whether a given expression node can be evaluated locally, assumes <see langword="true"/> if no function defined.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(this Expression expression, Func<Expression, bool>? canBeEvaluatedLocally = null)
            => new SubtreeEvaluator(new Nominator(canBeEvaluatedLocally).Nominate(expression.CheckNotNull(nameof(expression)))).Eval(expression);

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Block:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Default:
                case ExpressionType.Label:
                case ExpressionType.Goto:
                case ExpressionType.Lambda:
                case ExpressionType.Loop:
                case ExpressionType.New:
                case ExpressionType.Parameter:
                case ExpressionType.Quote:
                case ExpressionType.Throw:
                    return false;

                case ExpressionType.Constant:
                    {
                        var type = expression.Type;
                        if (type.GetCustomAttribute<QueryArgumentAttribute>() is not null ||
                            type.GetInterfaces().Any(x => x.GetCustomAttribute<QueryArgumentAttribute>() is not null))
                        {
                            return false;
                        }

                        if (type == typeof(CancellationToken))
                        {
                            return false;
                        }
                    }

                    break;

                case ExpressionType.Call:
                    {
                        if (expression.Type == typeof(void))
                        {
                            return false;
                        }

                        var methodCallExpression = (MethodCallExpression)expression;
                        if (methodCallExpression.Method.GetCustomAttribute<QueryMarkerFunctionAttribute>() is not null)
                        {
                            return false;
                        }

                        var methodDeclaringType = methodCallExpression.Method.DeclaringType;
                        if ((methodDeclaringType == typeof(Queryable) || methodDeclaringType == typeof(Enumerable)) &&
                            methodCallExpression.Arguments.FirstOrDefault() is ConstantExpression argument &&
                            argument.Value.AsQueryableResourceTypeOrNull() is not null)
                        {
                            return false;
                        }
                    }

                    break;

                case ExpressionType.MemberAccess:
                    if (expression is MemberExpression m && m.Type.Implements(typeof(IQueryable<>)))
                    {
                        if (m.Expression?.Type.Implements(typeof(Closure<>)) is true)
                        {
                            return false;
                        }
                    }

                    break;
            }

            return true;
        }

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down).
        /// </summary>
        private sealed class SubtreeEvaluator : SystemExpressionVisitorBase
        {
            private readonly HashSet<Expression> _candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
                => _candidates = candidates;

            internal Expression Eval(Expression expression) => Visit(expression);

            [return: NotNullIfNotNull("node")]
            protected override Expression? Visit(Expression? node)
            {
                if (node is null)
                {
                    return null;
                }

                if (_candidates.Contains(node))
                {
                    return Evaluate(node);
                }

                return base.Visit(node);
            }

            private static Expression Evaluate(Expression expression)
            {
                if (expression is ConstantExpression constantExpression)
                {
                    var enforceLocalEvaluation =
                        constantExpression.Type.IsInterface &&
                        constantExpression.Type.Implements(typeof(IEnumerable<>)) &&
                        constantExpression.Value?.GetType().DeclaringType == typeof(Enumerable);
                    if (!enforceLocalEvaluation)
                    {
                        return expression;
                    }
                }

                var value = expression.CompileAndInvokeExpression();

                if (value is IRemoteLinqQueryable remoteLinqQueryable)
                {
                    // unwrap subquery from closure
                    return remoteLinqQueryable.Expression;
                }

                if (value is Expression valueAsExpression)
                {
                    return valueAsExpression;
                }

                if (value is System.Collections.IEnumerable)
                {
                    var collectionType = value.GetType();
                    var elementType = TypeHelper.GetElementType(collectionType);
                    if (expression.Type.IsAssignableFrom(elementType.MakeArrayType()))
                    {
                        var enumerated = MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, new[] { value });
                        value = enumerated;
                    }
                    else if (value is EnumerableQuery && expression.Type.IsAssignableFrom(typeof(IQueryable<>).MakeGenericType(elementType)))
                    {
                        var enumerated = MethodInfos.Enumerable.ToArray.MakeGenericMethod(elementType).Invoke(null, new[] { value });
                        var queryable = MethodInfos.Queryable.AsQueryable.MakeGenericMethod(elementType).Invoke(null, new[] { enumerated });
                        value = queryable;
                    }
                }

                return Expression.Property(
                    Expression.New(
                        typeof(VariableQueryArgument<>).MakeGenericType(expression.Type).GetConstructor(new[] { expression.Type })!,
                        Expression.Constant(value, expression.Type)),
                    nameof(VariableQueryArgument<object>.Value));
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        private sealed class Nominator : SystemExpressionVisitorBase
        {
            private readonly object _lock = new();
            private readonly Func<Expression, bool> _canBeEvaluated;
            private HashSet<Expression>? _candidates;
            private bool _cannotBeEvaluated;

            internal Nominator(Func<Expression, bool>? canBeEvaluated)
                => _canBeEvaluated = canBeEvaluated.And(CanBeEvaluatedLocally)!;

            internal HashSet<Expression> Nominate(Expression expression)
            {
                lock (_lock)
                {
                    _candidates = new HashSet<Expression>();
                    Visit(expression);
                    return _candidates;
                }
            }

            [return: NotNullIfNotNull("expression")]
            protected override Expression? Visit(Expression? node)
            {
                if (node is not null)
                {
                    bool saveCannotBeEvaluated = _cannotBeEvaluated;
                    _cannotBeEvaluated = false;

                    base.Visit(node);

                    if (!_cannotBeEvaluated)
                    {
                        if (_canBeEvaluated(node))
                        {
                            _candidates!.Add(node);
                        }
                        else
                        {
                            _cannotBeEvaluated = true;
                        }
                    }

                    _cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return node;
            }
        }
    }
}