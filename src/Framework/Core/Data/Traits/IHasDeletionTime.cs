using System;

namespace ChengYuan.Core.Data;

public interface IHasDeletionTime
{
    DateTimeOffset? DeletionTime { get; }
}
