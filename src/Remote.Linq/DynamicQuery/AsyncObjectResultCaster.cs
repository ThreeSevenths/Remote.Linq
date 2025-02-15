﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.DynamicQuery
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous query result mapper using type casting.
    /// </summary>
    public sealed class AsyncObjectResultCaster : IAsyncQueryResultMapper<object>
    {
        private readonly ObjectResultCaster _objectResultCaster = new ObjectResultCaster();

        /// <inheritdoc/>
        public ValueTask<TResult> MapResultAsync<TResult>(object? source, Expression expression, CancellationToken cancellation = default)
            => new(_objectResultCaster.MapResult<TResult>(source, expression)!);
    }
}