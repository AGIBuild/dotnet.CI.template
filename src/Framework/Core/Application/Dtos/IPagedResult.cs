namespace ChengYuan.Core.Application.Dtos;

public interface IPagedResult<out T> : IListResult<T>
{
    long TotalCount { get; }
}
