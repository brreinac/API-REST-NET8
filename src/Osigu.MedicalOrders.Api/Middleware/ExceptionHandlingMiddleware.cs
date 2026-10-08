using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Osigu.MedicalOrders.Domain.Exceptions;

namespace Osigu.MedicalOrders.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException validationException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Validation error",
                    validationException.Message,
                    validationException.Errors
                ),

            NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    exception.Message,
                    null
                ),

            DomainException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Domain rule violation",
                    exception.Message,
                    null
                ),

            ArgumentException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    exception.Message,
                    null
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.",
                    null
                )
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status {StatusCode} for {Method} {Path}.",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem),
            context.RequestAborted);
    }
}
