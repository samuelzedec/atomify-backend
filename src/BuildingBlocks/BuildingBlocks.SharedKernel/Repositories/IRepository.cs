using BuildingBlocks.SharedKernel.Abstractions;

namespace BuildingBlocks.SharedKernel.Repositories;

/// <summary>
/// Define um contrato genérico para operações de repositório, permitindo a interação
/// com entidades no contexto de armazenamento de dados.
/// </summary>
/// <typeparam name="TEntity">
/// O tipo da entidade gerenciada pelo repositório. Deve ser uma derivada da classe <see cref="Entity"/>.
/// </typeparam>
/// <remarks>
/// Esta interface fornece métodos para criar, atualizar e excluir entidades, abstraindo a lógica
/// de acesso aos dados para as implementações específicas.
/// </remarks>
public interface IRepository<TEntity> where TEntity : Entity, IAggregateRoot
{
    /// <summary>
    /// Cria uma nova entidade no repositório de dados de forma assíncrona.
    /// </summary>
    /// <param name="entity">
    /// A entidade a ser criada no repositório. Deve ser uma instância do tipo <typeparamref name="TEntity"/>,
    /// que herda da classe <see cref="Entity"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona.
    /// </returns>
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca uma entidade existente para atualização no repositório de dados.
    /// </summary>
    /// <param name="entity">
    /// A entidade a ser atualizada. Deve ser uma instância do tipo <typeparamref name="TEntity"/>,
    /// que herda da classe <see cref="Entity"/>.
    /// </param>
    void Update(TEntity entity);

    /// <summary>
    /// Recupera uma entidade do repositório pelo identificador fornecido.
    /// </summary>
    /// <param name="id">
    /// O identificador único da entidade a ser recuperada. Deve ser um valor <see cref="Guid"/> correspondente ao <see cref="Entity.Id"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona. O resultado contém a entidade do tipo <typeparamref name="TEntity"/>
    /// correspondente ao identificador fornecido ou <c>null</c> caso nenhuma entidade seja encontrada.
    /// </returns>
    Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}