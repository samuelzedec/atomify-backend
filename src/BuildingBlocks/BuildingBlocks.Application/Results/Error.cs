using System.Text.Json.Serialization;

namespace BuildingBlocks.Application.Results;

public sealed class Error
{
    public string Message { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Code { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ErrorType? Type { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, string[]>? Details { get; init; }

    [JsonConstructor]
    public Error() { }

    public Error(
        ErrorType type,
        string message,
        string? code = null,
        Dictionary<string, string[]>? details = null)
    {
        Type = type;
        Message = message;
        Code = code;
        Details = details;
    }

    public static Error NotFound(string message, string? code = null)
        => new(ErrorType.NotFound, message, code);

    public static Error BadRequest(string message, string? code = null)
        => new(ErrorType.BadRequest, message, code);

    public static Error Conflict(string message, string? code = null)
        => new(ErrorType.Conflict, message, code);

    public static Error Unauthorized(string message, string? code = null)
        => new(ErrorType.Unauthorized, message, code);

    public static Error Forbidden(string message, string? code = null)
        => new(ErrorType.Forbidden, message, code);

    public static Error InternalServer(string message, string? code = null)
        => new(ErrorType.InternalServerError, message, code);
}