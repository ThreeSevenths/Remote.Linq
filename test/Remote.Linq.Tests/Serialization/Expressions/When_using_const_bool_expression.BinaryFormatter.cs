﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if !NETCOREAPP1_0

namespace Remote.Linq.Tests.Serialization.Expressions
{
    partial class When_using_const_bool_expression
    {
        public class BinaryFormatter : When_using_const_bool_expression
        {
            public BinaryFormatter()
                : base(BinarySerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif