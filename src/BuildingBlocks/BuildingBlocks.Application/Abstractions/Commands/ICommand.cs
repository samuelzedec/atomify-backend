using BuildingBlocks.Application.Results;

namespace BuildingBlocks.Application.Abstractions.Commands;

/// <summary>
/// Define uma interface que representa um comando no padrão CQRS (Command Query Responsibility Segregation).
/// A implementação desta interface é utilizada para encapsular uma solicitação que pode ser processada
/// para modificar o estado do sistema.
/// </summary>
/// <remarks>
/// A interface <see cref="ICommand"/> é genérica e retorna um resultado do tipo <see cref="Result"/>,
/// que indica o sucesso ou falha da operação, bem como informações associadas a possíveis erros.
/// </remarks>
public interface ICommand : Mediator.ICommand<Result>;

/// <summary>
/// Representa uma interface base para definição de comandos no padrão CQRS (Command Query Responsibility Segregation),
/// sendo utilizada para encapsular lógica de negócio destinada a alterar o estado do sistema.
/// </summary>
/// <remarks>
/// A implementação desta interface deve ser derivada para representar comandos específicos que possam ser
/// processados por um manipulador de comando (Command Handler), garantindo assim a separação entre
/// operações de escrita e leitura.
/// </remarks>
public interface ICommand<TCommandResponse> : Mediator.ICommand<Result<TCommandResponse>>
    where TCommandResponse : class;
