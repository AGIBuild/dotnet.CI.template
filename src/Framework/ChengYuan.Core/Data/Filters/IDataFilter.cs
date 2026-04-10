using System;

namespace ChengYuan.Core.Data;

public interface IDataFilter<TFilter>
{
    bool IsEnabled { get; }

    IDisposable Enable();

    IDisposable Disable();
}
