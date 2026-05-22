namespace BuildingBlocks.SharedKernel.Exceptions;

/// <summary>
/// Exceção lançada quando uma ou mais regras de validação são violadas,
/// encapsulando os erros agrupados por campo para facilitar o retorno estruturado ao cliente.
/// </summary>
public sealed class AtomifyValidationException(Dictionary<string, string[]> errors)
    : Exception("Dados inválidos.")
{
    public Dictionary<string, string[]> Errors => errors;
}