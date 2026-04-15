using System.Collections.Generic;

namespace ChengYuan.Core.Application.Dtos;

public interface IListResult<out T>
{
    IReadOnlyList<T> Items { get; }
}
