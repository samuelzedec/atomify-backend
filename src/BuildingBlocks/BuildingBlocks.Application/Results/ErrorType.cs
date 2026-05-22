namespace BuildingBlocks.Application.Results;

/// <summary>
/// Enumeração que define os tipos de erros que podem ocorrer em uma aplicação.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Erro de validação de dados.
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Recurso não encontrado.
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Conflito de dados ou estado.
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Não autenticado.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Não autorizado/sem permissão.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Erro indicando que a entidade enviada possui dados inválidos ou está em um estado que impede o processamento.
    /// </summary>
    UnprocessableEntity = 422,

    /// <summary>
    /// Indica que o cliente enviou um número excessivo de solicitações em um curto período de tempo.
    /// </summary>
    TooManyRequests = 429,

    /// <summary>
    /// Erro ocorrido devido a um problema interno no servidor.
    /// </summary>
    InternalServerError = 500
}