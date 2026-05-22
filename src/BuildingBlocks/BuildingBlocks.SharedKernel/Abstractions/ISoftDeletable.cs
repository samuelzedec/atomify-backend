namespace BuildingBlocks.SharedKernel.Abstractions;

/// <summary>
/// Interface que define o contrato para entidades que suportam exclusão lógica
/// (soft delete), permitindo rastrear o momento da exclusão sem removê-las do armazenamento.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Representa a data e hora em que a entidade foi marcada como excluída logicamente.
    /// Um valor nulo indica que a entidade não foi excluída.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }
    public Guid? DeletedBy { get; }
}