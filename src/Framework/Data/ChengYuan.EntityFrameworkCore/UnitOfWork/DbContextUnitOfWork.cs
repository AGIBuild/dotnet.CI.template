using System.Collections.Generic;
using System.Linq;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChengYuan.EntityFrameworkCore;

public sealed class DbContextUnitOfWork(DbContext dbContext) : IUnitOfWork, IDbContextUnitOfWorkParticipant, IUnitOfWorkInitializer
{
    private IDbContextTransaction? _transaction;

    public UnitOfWorkOptions Options { get; private set; } = UnitOfWorkOptions.Default;

    public bool IsCompleted { get; private set; }

    public bool IsRolledBack { get; private set; }

    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var trackedEntities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        List<IDomainEvent> events = [];

        foreach (var entity in trackedEntities)
        {
            events.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        return events;
    }

    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!Options.IsTransactional || !dbContext.Database.IsRelational() || dbContext.Database.CurrentTransaction is not null)
        {
            return;
        }

        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(dbContext.SaveChangesAsync(cancellationToken));
    }

    public async ValueTask CompleteAsync(CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);
        await SaveChangesAsync(cancellationToken);
        await CommitAsync(cancellationToken);
        IsCompleted = true;
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            IsRolledBack = true;
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
        IsRolledBack = true;
    }

    public void Initialize(UnitOfWorkOptions options)
    {
        Options = options;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
