﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.Tests.Serialization.Expressions
{
    using Remote.Linq.Expressions;
    using System;
    using Xunit;

    public abstract class When_using_simple_bool_expression
    {
        public class With_binary_formatter : When_using_simple_bool_expression
        {
            public With_binary_formatter()
                : base(BinarySerializationHelper.Clone)
            {
            }
        }

        public class With_data_contract_serializer : When_using_simple_bool_expression
        {
            public With_data_contract_serializer()
                : base(DataContractSerializationHelper.CloneExpression)
            {
            }
        }

        public class With_newtonsoft_json_serializer : When_using_simple_bool_expression
        {
            public With_newtonsoft_json_serializer()
                : base(NewtonsoftJsonSerializationHelper.Clone)
            {
            }
        }

        public class With_system_text_json_serializer : When_using_simple_bool_expression
        {
            public With_system_text_json_serializer()
                : base(SystemTextJsonSerializationHelper.Clone)
            {
            }
        }

#if NETFRAMEWORK
        public class With_net_data_contract_serializer : When_using_simple_bool_expression
        {
            public With_net_data_contract_serializer()
                : base(NetDataContractSerializationHelper.Clone)
            {
            }
        }
#endif // NETFRAMEWORK

        public class With_protobuf_net_serializer : When_using_simple_bool_expression
        {
            public With_protobuf_net_serializer()
                : base(ProtobufNetSerializationHelper.Clone)
            {
            }
        }

        public class With_xml_serializer : When_using_simple_bool_expression
        {
            public With_xml_serializer()
                : base(XmlSerializationHelper.CloneExpression)
            {
            }
        }

        private readonly LambdaExpression _remoteExpression;

        private readonly LambdaExpression _serializedRemoteExpression;

        protected When_using_simple_bool_expression(Func<LambdaExpression, LambdaExpression> serialize)
        {
            System.Linq.Expressions.Expression<Func<bool, bool>> expression = x => !x;

            _remoteExpression = expression.ToRemoteLinqExpression();

            _serializedRemoteExpression = serialize(_remoteExpression);
        }

        [Fact]
        public void Remote_expression_should_be_equal()
        {
            _remoteExpression.ShouldEqualRemoteExpression(_serializedRemoteExpression);
        }

        [Fact]
        public void System_expresison_should_be_equal()
        {
            var exp1 = _remoteExpression.ToLinqExpression<bool, bool>();
            var exp2 = _serializedRemoteExpression.ToLinqExpression<bool, bool>();

            exp1.ShouldEqualExpression(exp2);
        }
    }
}