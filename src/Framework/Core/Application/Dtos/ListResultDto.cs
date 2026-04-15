using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Application.Dtos;

public class ListResultDto<T> : IListResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public ListResultDto(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = items;
    }

    public ListResultDto(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = items is IReadOnlyList<T> list ? list : new List<T>(items);
    }
}
