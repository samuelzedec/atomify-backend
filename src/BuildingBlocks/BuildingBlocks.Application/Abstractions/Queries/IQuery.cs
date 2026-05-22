using BuildingBlocks.Application.Results;

namespace BuildingBlocks.Application.Abstractions.Queries;

/// <summary>
/// Define um contrato para consultas que retornam um resultado encapsulado de um tipo especificado.
/// </summary>
/// <typeparam name="TQuery">O tipo associado ao resultado da consulta.</typeparam>
public interface IQuery<TQuery> : Mediator.IQuery<Result<TQuery>>
    where TQuery : class;