using BuildingBlocks.SharedKernel.Exceptions;
using FluentValidation;
using Mediator;

namespace BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    private const int NoValidationErrors = 0;

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TRequest>(message);
        var validationErrors = validators
            .Select(v => v.Validate(context))
            .Where(vr => !vr.IsValid)
            .SelectMany(vr => vr.Errors)
            .GroupBy(vf => vf.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );

        return validationErrors.Count > NoValidationErrors
            ? throw new AtomifyValidationException(validationErrors)
            : await next(message, cancellationToken);
    }
}