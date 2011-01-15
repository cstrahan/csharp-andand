using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AndAnd
{
    public class AndAndVisitor : ExpressionVisitor
    {
        private List<ParameterExpression> _variables = new List<ParameterExpression>();
        private List<Expression> _assignments = new List<Expression>();
        private LabelTarget _returnLabelTarget = Expression.Label("skipToEnd");
        private int varCounter = 0;
        private ParameterExpression _isSafeToContinueVariable;

        private Expression CreateIntermediateVariable(Expression expression, MemberInfo memberInfo)
        {
            var ifExpr = Expression.IfThen(
                Expression.Equal(expression, Expression.Constant(null)),
                Expression.Block(
                    Expression.Assign(_isSafeToContinueVariable, Expression.Constant(false, typeof(bool))),
                    Expression.Return(_returnLabelTarget)
                )
            );

            _assignments.Add(ifExpr);

            var access = Expression.MakeMemberAccess(expression, memberInfo);
            var variable = Expression.Variable(GetMemberType(memberInfo), "var" + varCounter++);
            _variables.Add(variable);

            var assign = Expression.Assign(variable, access);
            _assignments.Add(assign);

            return variable;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            Type memberType = null;
            if (memberInfo is FieldInfo)
            {
                memberType = (memberInfo as FieldInfo).FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                memberType = (memberInfo as PropertyInfo).PropertyType;
            }
            else if (memberInfo is MethodInfo)
            {
                memberType = (memberInfo as MethodInfo).ReturnType;
            }

            return memberType;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            // A chain of MemberExpressions will be in "reverse" - so let's sort them.
            var memberExpressionChain = GetRecursiveInclusive(m, mem => mem.Expression as MemberExpression).Reverse().ToList();
            var firstMemberExpression = memberExpressionChain.First();
            var previousExpression = Visit(firstMemberExpression.Expression);

            foreach (var memberExpression in memberExpressionChain)
            {
                previousExpression = CreateIntermediateVariable(previousExpression, memberExpression.Member);
            }

            return previousExpression;
        }

        private static IEnumerable<T> GetRecursiveInclusive<T>(T recursiveLike, Func<T, T> selector) where T : class
        {
            T next = recursiveLike;
            yield return next;
            while (true)
            {
                next = selector(next);
                if (next == null) yield break;
                yield return next;
            }
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var returnType = lambda.ReturnType;
            _isSafeToContinueVariable = Expression.Variable(typeof(bool), "isSafeToContinue");
            _variables.Add(_isSafeToContinueVariable);
            var initIsSafeToContinueVariable = Expression.Assign(_isSafeToContinueVariable, Expression.Constant(true, typeof(bool)));

            var condition = Expression.Condition(
                _isSafeToContinueVariable,
                Visit(lambda.Body),
                Expression.Constant(null, returnType)
                );
            
            var body = new List<Expression>();
            body.Add(initIsSafeToContinueVariable);
            body.AddRange(_assignments);
            body.Add(Expression.Label(_returnLabelTarget));
            body.Add(condition);

            return Expression.Lambda(
                Expression.Block(
                    returnType,
                    _variables,
                    body
                ),
                lambda.Parameters
            );
        }
    }
}