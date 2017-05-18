﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.ExpressionVisitors
{
    using Remote.Linq.Expressions;
    using Aqua.TypeSystem;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class RemoteExpressionVisitorBase
    {
        protected readonly ITypeResolver _typeResolver;

        protected RemoteExpressionVisitorBase(ITypeResolver typeResolver = null)
        {
            _typeResolver = typeResolver ?? TypeResolver.Instance;
        }

        protected virtual Expression Visit(Expression expression)
        {
            if (ReferenceEquals(null, expression))
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Binary:
                    return VisitBinary((BinaryExpression)expression);

                case ExpressionType.Block:
                    return VisitBlock((BlockExpression)expression);

                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)expression);

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)expression);

                case ExpressionType.Unary:
                    return VisitUnary((UnaryExpression)expression);

                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);

                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);

                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)expression);

                case ExpressionType.New:
                    return VisitNew((NewExpression)expression);

                case ExpressionType.NewArray:
                    return VisitNewArray((NewArrayExpression)expression);

                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)expression);

                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)expression);

                case ExpressionType.Goto:
                    return VisitGoto((GotoExpression)expression);

                case ExpressionType.Label:
                    return VisitLabel((LabelExpression)expression);

                default:
                    throw new Exception(string.Format("Unknown expression type: '{0}'", expression.NodeType));
            }
        }

        private Expression VisitLabel(LabelExpression labelExpression)
        {
            Expression expression = Visit(labelExpression.Expression);
            if (!ReferenceEquals(expression, labelExpression.Expression))
            {
                return new LabelExpression(labelExpression.LabelTarget,expression);
            }
            return labelExpression;
        }

        protected virtual Expression VisitGoto(GotoExpression gotoExpression)
        {
            Expression expression = Visit(gotoExpression.Expression);
            if (!ReferenceEquals(expression, gotoExpression.Expression))
            {
                return new GotoExpression(gotoExpression.Kind,expression,gotoExpression.Target,gotoExpression.Type);
            }
            return gotoExpression;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);

            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return new MemberInitExpression(n, bindings);
            }

            return init;
        }

        protected virtual List<MemberBinding> VisitBindingList(List<MemberBinding> original)
        {
            if (ReferenceEquals(null, original))
            {
                return null;
            }

            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);

                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);

                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);

                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = Visit(assignment.Expression);

            if (e != assignment.Expression)
            {
                return new MemberAssignment(assignment.Member, e);
            }

            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings.ToList());

            if (bindings != binding.Bindings)
            {
                return new MemberMemberBinding(binding.Member, bindings);
            }

            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);

            if (!ReferenceEquals(initializers, binding.Initializers))
            {
                return new MemberListBinding(binding.Member, initializers);
            }

            return binding;
        }

        protected virtual List<ElementInit> VisitElementInitializerList(List<ElementInit> original)
        {
            if (ReferenceEquals(null, original))
            {
                return null;
            }

            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            IEnumerable<Expression> arguments = VisitExpressionList(initializer.Arguments);

            if (!ReferenceEquals(arguments, initializer.Arguments))
            {
                return new ElementInit(initializer.AddMethod.Method, arguments);
            }

            return initializer;
        }

        protected virtual List<T> VisitExpressionList<T>(List<T> original) where T: Expression
        {
            if (ReferenceEquals(null, original))
            {
                return null;
            }

            List<T> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                T p = (T)Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<T>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(p);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        protected virtual NewExpression VisitNew(NewExpression newExpression)
        {
            var args = VisitExpressionList(newExpression.Arguments);

            if (!ReferenceEquals(args, newExpression.Arguments))
            {
                return new NewExpression(newExpression.Constructor, args, newExpression.Members);
            }

            return newExpression;
        }

        private Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            var expressions = VisitExpressionList(newArrayExpression.Expressions);

            if (!ReferenceEquals(expressions, newArrayExpression.Expressions))
            {
                return new NewArrayExpression(newArrayExpression.NewArrayType, newArrayExpression.Type, expressions);
            }

            return newArrayExpression;
        }

        protected virtual Expression VisitListInit(ListInitExpression listInitExpression)
        {
            var newExpression = VisitNew(listInitExpression.NewExpression);

            if (!ReferenceEquals(newExpression, listInitExpression.NewExpression))
            {
                return new ListInitExpression(newExpression, listInitExpression.Initializers);
            }

            return listInitExpression;
        }

        protected virtual Expression VisitBinary(BinaryExpression expression)
        {
            var leftOperand = Visit(expression.LeftOperand);
            var rightOperand = Visit(expression.RightOperand);
            var conversion = Visit(expression.Conversion) as LambdaExpression;

            if (!ReferenceEquals(leftOperand, expression.LeftOperand) || !ReferenceEquals(rightOperand, expression.RightOperand) || !ReferenceEquals(conversion, expression.Conversion))
            {
                return new BinaryExpression(expression.BinaryOperator, leftOperand, rightOperand, expression.IsLiftedToNull, expression.Method, conversion);
            }

            return expression;
        }

        protected virtual Expression VisitBlock(BlockExpression node)
        {
            var variables = VisitExpressionList(node.Variables);
            var expressions = VisitExpressionList(node.Expressions);
            
            if (!ReferenceEquals(variables, node.Variables) || !ReferenceEquals(expressions, node.Expressions))
            {
                return new BlockExpression(node.Type, variables, expressions);
            }

            return node;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression expression)
        {
            var exp = Visit(expression.Expression);

            if (!ReferenceEquals(exp, expression.Expression))
            {
                var type = _typeResolver.ResolveType(expression.TypeOperand);
                return new TypeBinaryExpression(exp, type);
            }

            return expression;
        }

        protected virtual Expression VisitConditional(ConditionalExpression expression)
        {
            var test = Visit(expression.Test);
            var ifTrue = Visit(expression.IfTrue);
            var ifFalse = Visit(expression.IfFalse);

            if (!ReferenceEquals(test, expression.Test) || !ReferenceEquals(ifTrue, expression.IfTrue) || !ReferenceEquals(ifFalse, expression.IfFalse))
            {
                return new ConditionalExpression(test, ifTrue, ifFalse);
            }

            return expression;
        }

        protected virtual ConstantExpression VisitConstant(ConstantExpression expression)
        {
            return expression;
        }

        protected virtual ParameterExpression VisitParameter(ParameterExpression expression)
        {
            return expression;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression expression)
        {
            var instance = Visit(expression.Expression);

            if (!ReferenceEquals(instance, expression.Expression))
            {
                return new MemberExpression(instance, expression.Member);
            }

            return expression;
        }

        protected virtual Expression VisitUnary(UnaryExpression expression)
        {
            var operand = Visit(expression.Operand);

            if (!ReferenceEquals(operand, expression.Operand))
            {
                return new UnaryExpression(expression.UnaryOperator, operand, expression.Type, expression.Method);
            }

            return expression;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression expression)
        {
            var instance = Visit(expression.Instance);
            var argumements =
                from i in expression.Arguments
                select new { Old = i, New = Visit(i) };

            argumements = argumements.ToList();

            if (!ReferenceEquals(instance, expression.Instance) || argumements.Any(i => !ReferenceEquals(i.Old, i.New)))
            {
                return new MethodCallExpression(instance, expression.Method, argumements.Select(i => i.New));
            }

            return expression;
        }

        protected virtual Expression VisitLambda(LambdaExpression expression)
        {
            var exp = Visit(expression.Expression);
            var parameters =
                from i in expression.Parameters
                select new { Old = i, New = VisitParameter(i) };

            parameters = parameters.ToList();

            if (!ReferenceEquals(exp, expression.Expression) || parameters.Any(i => !ReferenceEquals(i.Old, i.New)))
            {
                return new LambdaExpression(expression.Type, exp, parameters.Select(i => i.New));
            }

            return expression;
        }
    }
}
