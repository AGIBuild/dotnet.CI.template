using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Application.Dtos;

public class PagedResultDto<T> : ListResultDto<T>, IPagedResult<T>
{
    public long TotalCount { get; }

    public PagedResultDto(long totalCount, IReadOnlyList<T> items)
        : base(items)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        TotalCount = totalCount;
    }

    public PagedResultDto(long totalCount, IEnumerable<T> items)
        : base(items)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        TotalCount = totalCount;
    }
}
