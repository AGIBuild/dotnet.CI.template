using System;
using ChengYuan.Core.Guids;

namespace ChengYuan.ExecutionContext;

internal sealed class DefaultGuidGenerator : IGuidGenerator
{
    public Guid Create()
    {
        return Guid.NewGuid();
    }
}
