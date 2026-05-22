namespace BuildingBlocks.SharedKernel.Abstractions;

/// <summary>
/// Define um contrato para tipos que representam raízes de agregados no contexto do Domain-Driven Design (DDD).
/// </summary>
/// <remarks>
/// Uma raiz de agregado é uma entidade que atua como ponto central para gerenciar a consistência de um agregado.
/// Essa interface é utilizada para identificar objetos que controlam um conjunto de entidades coerentes e relacionadas,
/// aplicando regras de negócio e garantindo a integridade entre os seus membros.
/// </remarks>
public interface IAggregateRoot;