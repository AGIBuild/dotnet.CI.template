using ChengYuan.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Core.EntityFrameworkCore;

public sealed class DbContextUnitOfWork(DbContext dbContext) : IUnitOfWork
{
    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(dbContext.SaveChangesAsync(cancellationToken));
    }
}
