using System.Collections.Generic;
using System.Linq;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.EntityFrameworkCore;

internal sealed class CompositeDbContextUnitOfWork(
    IEnumerable<IDbContextUnitOfWorkParticipant> participants,
    IDomainEventPublisher domainEventPublisher) : IUnitOfWork, IUnitOfWorkInitializer
{
    private readonly IReadOnlyList<IDbContextUnitOfWorkParticipant> _participants = participants.ToArray();

    public UnitOfWorkOptions Options { get; private set; } = UnitOfWorkOptions.Default;

    public bool IsCompleted { get; private set; }

    public bool IsRolledBack { get; private set; }

    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfCompleted();

        foreach (var participant in _participants)
        {
            await participant.BeginTransactionAsync(cancellationToken);
        }

        List<IDomainEvent> allEvents = [];

        foreach (var participant in _participants)
        {
            allEvents.AddRange(participant.CollectDomainEvents());
        }

        foreach (var participant in _participants)
        {
            await participant.SaveChangesAsync(cancellationToken);
        }

        if (allEvents.Count > 0)
        {
            await domainEventPublisher.PublishAsync(allEvents, cancellationToken);

            foreach (var participant in _participants)
            {
                await participant.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async ValueTask CompleteAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfCompleted();

        try
        {
            foreach (var participant in _participants)
            {
                await participant.BeginTransactionAsync(cancellationToken);
            }

            await SaveChangesAsync(cancellationToken);

            foreach (var participant in _participants)
            {
                await participant.CommitAsync(cancellationToken);
            }

            IsCompleted = true;
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (IsCompleted || IsRolledBack)
        {
            return;
        }

        foreach (var participant in _participants)
        {
            await participant.RollbackAsync(cancellationToken);
        }

        IsRolledBack = true;
    }

    public void Initialize(UnitOfWorkOptions options)
    {
        Options = options;

        foreach (var participant in _participants)
        {
            if (participant is IUnitOfWorkInitializer initializer)
            {
                initializer.Initialize(options);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var participant in _participants)
        {
            if (participant is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }

    public void Dispose()
    {
        foreach (var participant in _participants)
        {
            if (participant is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private void ThrowIfCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("The unit of work has already been completed.");
        }

        if (IsRolledBack)
        {
            throw new InvalidOperationException("The unit of work has already been rolled back.");
        }
    }
}
