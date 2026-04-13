using System;
using System.Linq.Expressions;

namespace ChengYuan.Core.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();

    bool IsSatisfiedBy(T entity);
}
