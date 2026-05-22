namespace BuildingBlocks.SharedKernel.Repositories;

/// <summary>
/// Representa uma unidade de trabalho que encapsula um conjunto de mudanças para persistência.
/// Fornece mecanismos para gerenciar transações e repositórios de forma coesa.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Salva todas as mudanças feitas no contexto para o banco de dados de forma assíncrona.
    /// </summary>
    /// <param name="cancellationToken">Um token para monitorar solicitações de cancelamento.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia uma nova transação de banco de dados de forma assíncrona.
    /// Esta transação encapsula uma série de operações que podem ser confirmadas ou revertidas como uma unidade.
    /// </summary>
    /// <param name="cancellationToken">Token para monitorar solicitações de cancelamento.</param>
    /// <returns>Uma tarefa representando a operação assíncrona.</returns>
    ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma a transação atual de forma assíncrona, persistindo todas as alterações realizadas.
    /// </summary>
    /// <param name="cancellationToken">Token para monitorar solicitações de cancelamento.</param>
    /// <returns>Uma tarefa representando a operação assíncrona.</returns>
    ValueTask CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverte todas as operações realizadas na transação atual de forma assíncrona.
    /// Esta operação é usada para cancelar todas as mudanças não confirmadas no contexto persistente.
    /// </summary>
    /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
    ValueTask RollbackTransactionAsync();
}