﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.Async.Queryable.Expressions
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Remote.Linq.Async.Queryable.ExpressionExecution;
    using Remote.Linq.Expressions;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExpressionAsyncExecutionExtensions
    {
        /// <summary>
        /// Composes and executes the query based on the <see cref="Expression"/> and mappes the result into dynamic objects.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to be executed.</param>
        /// <param name="asyncQueryableProvider">Delegate to provide <see cref="IAsyncQueryable"/> instances for given <see cref="Type"/>s.</param>
        /// <param name="context">Optional instance of <see cref="IExpressionFromRemoteLinqContext"/> to be used to translate <see cref="TypeInfo"/>, <see cref="Expression"/> etc.</param>
        /// <param name="setTypeInformation">Function to define whether to add type information.</param>
        /// <param name="cancellation">A <see cref="CancellationToken" /> to observe while waiting for the async operation to complete.</param>
        /// <returns>The asynchronous query result.</returns>
        public static ValueTask<DynamicObject?> ExecuteAsync(
            this Expression expression,
            Func<Type, IAsyncQueryable> asyncQueryableProvider,
            IExpressionFromRemoteLinqContext? context = null,
            Func<Type, bool>? setTypeInformation = null,
            CancellationToken cancellation = default)
            => new DefaultReactiveAsyncExpressionExecutor(asyncQueryableProvider, context, setTypeInformation).ExecuteAsync(expression, cancellation);

        /// <summary>
        /// Composes and executes the query based on the <see cref="Expression"/> as an asynchronous operation.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to be executed.</param>
        /// <param name="asyncQueryableProvider">Delegate to provide <see cref="IAsyncQueryable"/> instances for given <see cref="Type"/>s.</param>
        /// <param name="context">Optional instance of <see cref="IExpressionFromRemoteLinqContext"/> to be used to translate <see cref="TypeInfo"/>, <see cref="Expression"/> etc.</param>
        /// <param name="cancellation">A <see cref="CancellationToken" /> to observe while waiting for the async operation to complete.</param>
        /// <returns>The asynchronous query result.</returns>
        public static ValueTask<TResult?> ExecuteAsync<TResult>(
            this Expression expression,
            Func<Type, IAsyncQueryable> asyncQueryableProvider,
            IExpressionFromRemoteLinqContext? context = null,
            CancellationToken cancellation = default)
            => new CastingReactiveAsyncExpressionExecutor<TResult>(asyncQueryableProvider, context).ExecuteAsync(expression, cancellation);
    }
}