using BuildingBlocks.SharedKernel.Abstractions;

namespace BuildingBlocks.SharedKernel.Repositories;

/// <summary>
/// Contrato adicional para repositórios que suportam remoção física de entidades.
/// Deve ser implementado apenas por agregados que não utilizam soft delete.
/// </summary>
/// <typeparam name="TEntity">
/// O tipo da entidade gerenciada pelo repositório.
/// </typeparam>
public interface IRemovableRepository<in TEntity> 
    where TEntity : Entity, IAggregateRoot
{
    /// <summary>
    /// Marca uma entidade para remoção física do repositório de dados.
    /// </summary>
    /// <param name="entity">A entidade a ser removida.</param>
    void Delete(TEntity entity);
}
