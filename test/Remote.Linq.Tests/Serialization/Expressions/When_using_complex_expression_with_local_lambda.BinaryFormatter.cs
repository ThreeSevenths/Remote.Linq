﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETCOREAPP1_0

namespace Remote.Linq.Tests.Serialization.Expressions
{
    partial class When_using_complex_expression_with_local_lambda
    {
        public class BinaryFormatter : When_using_complex_expression_with_local_lambda
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif