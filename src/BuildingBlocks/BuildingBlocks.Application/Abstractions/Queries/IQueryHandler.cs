using BuildingBlocks.Application.Results;

namespace BuildingBlocks.Application.Abstractions.Queries;

/// <summary>
/// Define um manipulador para consultas (queries) no padrão CQRS, responsável por processar uma consulta
/// do tipo especificado e retornar uma resposta envelopada em um objeto <see cref="Result{T}"/>.
/// </summary>
/// <typeparam name="TQuery">
/// O tipo da consulta que será processada, o qual deve implementar a interface <see cref="IQuery&lt;TResponse&gt;"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// O tipo da resposta esperada após o processamento da consulta.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResponse> : Mediator.IQueryHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
    where TResponse : class;