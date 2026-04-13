using System.Linq;
using ChengYuan.Core.Specifications;

namespace ChengYuan.Core.Extensions;

public static class SpecificationQueryableExtensions
{
    public static IQueryable<T> Where<T>(this IQueryable<T> source, ISpecification<T> specification)
    {
        return source.Where(specification.ToExpression());
    }
}
