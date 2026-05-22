namespace BuildingBlocks.SharedKernel.Exceptions;

/// <summary>
/// Classe base para todas as exceções de domínio da aplicação Copy,
/// garantindo que erros de negócio sejam tipados e tratados de forma centralizada.
/// </summary>
public class AtomifyException(string message)
    : Exception(message);