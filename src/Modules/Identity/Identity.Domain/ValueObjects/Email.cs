using System.Text.RegularExpressions;
using BuildingBlocks.SharedKernel.Abstractions;
using BuildingBlocks.SharedKernel.Exceptions;

namespace Identity.Domain.ValueObjects;

/// <summary>
/// Representa um endereço de email com validações específicas para o domínio.
/// Garante que um email seja válido e respeite os requisitos de formato e comprimento.
/// </summary>
public sealed partial record Email : ValueObject
{
    public const int MaxLength = 255;
    public const string Regex = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";

    public string Value { get; }

    private Email(string value)
        => Value = value;

    /// <summary>
    /// Cria uma nova instância do valor do objeto Email com base no valor fornecido.
    /// </summary>
    /// <param name="value">O endereço de email que será validado e utilizado para criar o objeto.</param>
    /// <returns>Uma nova instância de <see cref="Email"/> com o valor validado.</returns>
    /// <exception cref="AtomifyException">
    /// Lançada quando o valor fornecido é vazio, ultrapassa o comprimento máximo permitido,
    /// ou não está em um formato válido de endereço de email.
    /// </exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AtomifyException("O email não pode ser vazio.");

        value = value.Trim().ToLowerInvariant();

        if (value.Length > MaxLength)
            throw new AtomifyException($"O email deve ter no máximo {MaxLength} caracteres.");

        return EmailRegex().IsMatch(value)
            ? new Email(value)
            : throw new AtomifyException("O email deve ter o formato válido.");
    }

    [GeneratedRegex(Regex)]
    private static partial Regex EmailRegex();

    public override string ToString()
        => Value;

    public static implicit operator string(Email email)
        => email.Value;
}