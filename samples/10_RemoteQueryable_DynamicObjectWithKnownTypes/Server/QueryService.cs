﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Server
{
    using Aqua.Dynamic;
    using Common;
    using Common.ServiceContracts;
    using Remote.Linq;
    using Remote.Linq.Expressions;

    public class QueryService : IQueryService
    {
        private InMemoryDataStore DataStore => InMemoryDataStore.Instance;

        private DynamicObjectMapper Mapper => new DynamicObjectMapper(isKnownTypeProvider: new IsKnownTypeProvider());

        public DynamicObject ExecuteQuery(Expression queryExpression)
            => queryExpression.Execute(DataStore.QueryableByTypeProvider, mapper: Mapper);
    }
}
