using Mediator;

namespace BuildingBlocks.SharedKernel.Abstractions;


/// <summary>
/// Representa um evento de domínio da aplicação.
/// Utilizado para sinalizar ocorrências importantes dentro do domínio.
/// </summary>
public interface IDomainEvent : INotification;