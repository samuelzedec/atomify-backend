namespace BuildingBlocks.Infrastructure.Persistence.Models;

public abstract class EntityData
{
    public Guid Id { get; protected init; }
    public DateTimeOffset CreatedAt { get; protected init; }
    public DateTimeOffset? UpdatedAt { get; protected init; }
}
