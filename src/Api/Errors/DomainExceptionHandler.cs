using Core;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
            return false;

        var result = Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Конфлікт бізнес-правил.",
            detail: "Операцію неможливо виконати через поточний стан даних.");

        await result.ExecuteAsync(httpContext);

        return true;
    }
}