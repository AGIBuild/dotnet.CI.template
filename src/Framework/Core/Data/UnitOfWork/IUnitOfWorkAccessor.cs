using System;

namespace ChengYuan.Core.Data;

public interface IUnitOfWorkAccessor
{
    IUnitOfWork? Current { get; }

    IDisposable Change(IUnitOfWork? unitOfWork);
}
