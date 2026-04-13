using System;
using System.Linq.Expressions;

namespace ChengYuan.Core.Specifications;

internal sealed class NotSpecification<T>(ISpecification<T> inner) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = inner.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.Not(Expression.Invoke(innerExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
