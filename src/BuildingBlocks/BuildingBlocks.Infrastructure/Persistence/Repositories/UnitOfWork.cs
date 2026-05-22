using BuildingBlocks.SharedKernel.Abstractions;
using BuildingBlocks.SharedKernel.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence.Repositories;

public abstract class UnitOfWork<TContext>(TContext context, IMediator mediator)
    : IUnitOfWork where TContext : DbContext
{
    protected readonly TContext _context = context;
    private IDbContextTransaction? _transaction;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events = ExtractAndClearEvents();
        await _context.SaveChangesAsync(cancellationToken);

        if (_transaction is null)
            await PublishEventsAsync(events, cancellationToken);
    }

    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transação já está ativa.");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async ValueTask CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Não há uma transação iniciada.");

        var events = ExtractAndClearEvents();
        await _context.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;

        await PublishEventsAsync(events, cancellationToken);
    }

    public async ValueTask RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(CancellationToken.None);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private List<IDomainEvent> ExtractAndClearEvents()
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.Events().Count != 0)
            .ToList();

        var events = entities.SelectMany(e => e.Entity.Events()).ToList();
        entities.ForEach(e => e.Entity.ClearEvents());
        return events;
    }

    private async Task PublishEventsAsync(List<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var evt in events)
            await mediator.Publish(evt, ct);
    }
}