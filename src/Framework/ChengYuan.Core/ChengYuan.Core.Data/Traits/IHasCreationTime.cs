using System;

namespace ChengYuan.Core.Data;

public interface IHasCreationTime
{
    DateTimeOffset CreationTime { get; }
}
