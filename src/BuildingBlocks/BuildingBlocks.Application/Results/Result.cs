using System.Text.Json.Serialization;

namespace BuildingBlocks.Application.Results;

/// <summary>
/// Representa o resultado de uma operação, indicando sucesso ou falha, e permite manipular
/// informações sobre erros associados a essa operação.
/// </summary>
public class Result
{
    public Error? Error { get; init; }
    [JsonIgnore] public bool IsSuccess { get; }
    [JsonIgnore] public bool IsFailure => !IsSuccess;

    [JsonConstructor]
    protected Result() { }

    protected Result(bool isSuccess, Error? error = null)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException();

        if (!isSuccess && error is null)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Cria uma nova instância da classe <see cref="Result"/> representando uma operação bem-sucedida.
    /// </summary>
    /// <returns>Uma instância de <see cref="Result"/> indicando sucesso e sem erros associados.</returns>
    public static Result Success()
        => new(isSuccess: true);

    /// <summary>
    /// Cria uma nova instância da classe <see cref="Result"/> representando uma operação falha.
    /// </summary>
    /// <param name="error">Um objeto <see cref="Error"/> que contém informações detalhadas sobre o erro ocorrido.</param>
    /// <returns>Uma instância de <see cref="Result"/> indicando falha e contendo o erro associado.</returns>
    public static Result Fail(Error error)
        => new(isSuccess: false, error);

    public static implicit operator Result(Error error)
        => Fail(error);
}

/// <summary>
/// Representa o resultado genérico de uma operação, proporcionando uma maneira de manipular
/// informações relacionadas ao sucesso ou falha da operação e de armazenar um valor associado.
/// </summary>
/// <typeparam name="T">O tipo do valor associado ao resultado da operação.</typeparam>
public sealed class Result<T> : Result
    where T : class
{
    public T? Value { get; init; }

    [JsonConstructor]
    private Result() { }

    private Result(bool isSuccess, Error? error = null, T? value = null) : base(isSuccess, error)
        => Value = value;

    /// <summary>
    /// Cria uma nova instância da classe <see cref="Result&lt;T&gt;"/> representando uma operação bem-sucedida.
    /// </summary>
    /// <param name="value">O valor associado ao resultado da operação.</param>
    /// <returns>Uma instância de <see cref="Result&lt;T&gt;"/> indicando sucesso e contendo o valor associado.</returns>
    public static Result<T> Ok(T value)
        => new(true, value: value);

    /// <summary>
    /// Cria uma nova instância da classe <see cref="Result&lt;T&gt;"/> representando uma operação mal-sucedida.
    /// </summary>
    /// <param name="error">A instância de <see cref="Error"/> contendo informações sobre o erro associado à falha da operação.</param>
    /// <returns>Uma instância de <see cref="Result&lt;T&gt;"/> indicando falha e contendo informações sobre o erro associado.</returns>
    public static new Result<T> Fail(Error error)
        => new(false, error);

    public static implicit operator Result<T>(T value)
        => Ok(value);

    public static implicit operator Result<T>(Error error)
        => Fail(error);
}