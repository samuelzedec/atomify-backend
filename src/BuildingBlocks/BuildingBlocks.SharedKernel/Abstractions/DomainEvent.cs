namespace BuildingBlocks.SharedKernel.Abstractions;

/// <summary>
/// Representa um evento de domínio base da aplicação.
/// </summary>
public abstract record DomainEvent(
    Guid? TenantId = null,
    Guid? CorrelationId = null) : IDomainEvent
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    public Guid EventId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Data e hora em que o evento ocorreu.
    /// </summary>
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}