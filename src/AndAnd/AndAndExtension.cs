using System;
using System.Linq.Expressions;

namespace AndAnd
{
    public static class AndAndExtension
    {
        public static TResult AndAnd<T, TResult>(this T obj, Expression<Func<T, TResult>> expression) where TResult : class
        {
            // Early-out
            if (obj == null) return null;

            var visitor = new AndAndVisitor();
            var newExpression = visitor.Visit(expression) as Expression<Func<T, TResult>>;
            return newExpression.Compile()(obj);
        }
    }
}