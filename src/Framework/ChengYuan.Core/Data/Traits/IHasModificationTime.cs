using System;

namespace ChengYuan.Core.Data;

public interface IHasModificationTime
{
    DateTimeOffset? LastModificationTime { get; }
}
