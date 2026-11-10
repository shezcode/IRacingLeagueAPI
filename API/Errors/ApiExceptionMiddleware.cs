using IRacingLeague.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IRacingLeague.API.Errors;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode >= 400)
                context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
            return Task.CompletedTask;
        });
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title, detail) = Map(exception);
            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled API exception. TraceId: {TraceId}", context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = status;
            await Results.Problem(
                statusCode: status,
                title: title,
                detail: detail,
                extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier })
                .ExecuteAsync(context);
        }
    }

    private static (int Status, string Title, string Detail) Map(Exception exception) => exception switch
    {
        NotFoundException or KeyNotFoundException => (404, "Not Found", exception.Message),
        ForbiddenException => (403, "Forbidden", exception.Message),
        ConflictException or UserOwnsLeaguesException => (409, "Conflict", exception.Message),
        DuplicateRegistrationException or LeagueFullException => (400, "Bad Request", exception.Message),
        DomainValidationException or ArgumentException => (400, "Bad Request", exception.Message),
        UnauthorizedAccessException => (401, "Unauthorized", exception.Message),
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => (409, "Conflict", "A resource with the same unique value already exists."),
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } => (409, "Conflict", "The resource is referenced by related records."),
        _ => (500, "Internal Server Error", "An unexpected error occurred.")
    };
}
