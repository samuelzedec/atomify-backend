using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Application.Results;
using BuildingBlocks.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Admin.Api.Middlewares;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string InternalServerErrorMessage =
        "Ocorreu um erro inesperado. Tente novamente mais tarde.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {ExceptionType} - {Message}",
            exception.GetType().Name, exception.Message);

        (ErrorType errorType, string message, Dictionary<string, string[]>? errors) = GetErrorDetails(exception);
        await WriteErrorResponseAsync(httpContext, errorType, message, errors, cancellationToken);
        return true;
    }

    private static (ErrorType, string, Dictionary<string, string[]>?) GetErrorDetails(Exception exception)
        => exception switch
        {
            AtomifyException ex => (ErrorType.UnprocessableEntity, ex.Message, null),
            AtomifyValidationException ex => (ErrorType.BadRequest, exception.Message, ex.Errors),
            _ => (ErrorType.InternalServerError, InternalServerErrorMessage, null)
        };

    private static async Task WriteErrorResponseAsync(
        HttpContext httpContext,
        ErrorType errorType,
        string message,
        Dictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        var response = new Error(errorType, message, null, errors);
        httpContext.Response.StatusCode = (int)errorType;
        httpContext.Response.ContentType = "application/json;";

        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            response,
            JsonOptions,
            cancellationToken
        );
    }
}