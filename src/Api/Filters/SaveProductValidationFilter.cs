using Api.Contracts;
using FluentValidation;

namespace Api.Filters;

public sealed class SaveProductValidationFilter(
    IValidator<SaveProductRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments
            .OfType<SaveProductRequest>()
            .Single();

        var result = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .Distinct()
                        .ToArray());

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}