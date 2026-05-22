using BuildingBlocks.Application.Results;

namespace BuildingBlocks.Application.Abstractions.Commands;

/// <summary>
/// Representa um manipulador de comandos, responsável por lidar com a execução de uma determinada
/// operação definida pelo comando fornecido.
/// </summary>
/// <typeparam name="TCommand">
/// O tipo do comando que será manipulado. Deve ser uma implementação da interface <see cref="ICommand"/>.
/// </typeparam>
public interface ICommandHandler<in TCommand> : Mediator.ICommandHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Representa um manipulador de comandos, responsável por lidar com a execução de uma
/// operação associada ao comando fornecido.
/// </summary>
/// <typeparam name="TCommand">
/// O tipo do comando que será manipulado. Deve ser uma implementação da interface <see cref="ICommand"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// O tipo do resultado retornado após o processamento do comando.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResponse> : Mediator.ICommandHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
    where TResponse : class;