using System;

namespace ChengYuan.Core.Application.Dtos;

public class PagedAndSortedResultRequestDto
{
    public const int DefaultMaxResultCount = 25;
    public const int MaxAllowedMaxResultCount = 1000;

    private int _maxResultCount = DefaultMaxResultCount;

    public int SkipCount { get; init; }

    public int MaxResultCount
    {
        get => _maxResultCount;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _maxResultCount = Math.Min(value, MaxAllowedMaxResultCount);
        }
    }

    public string? Sorting { get; init; }
}
