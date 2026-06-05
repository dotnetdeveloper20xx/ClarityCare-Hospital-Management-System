using System.Text.Json;
using ClarityCare.Application.Common.Exceptions;
using ClarityCare.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ClarityCare.Api.Middleware;

/// <summary>
/// Enterprise-grade global exception handling middleware.
/// 
/// Compliance Requirements:
/// - Maps all exceptions to RFC 7807 Problem Details responses
/// - NEVER exposes stack traces, inner exceptions, or system internals to clients
/// - Generates unique ErrorId for 500 errors enabling support correlation
/// - Logs full exception context internally with structured logging
/// - Maps DomainException to 422 Unprocessable Entity (business rule violation)
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? context.TraceIdentifier;

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                CreateValidationProblem(validationEx, correlationId)),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                CreateProblem(StatusCodes.Status404NotFound, "Resource Not Found",
                    notFoundEx.Message, correlationId)),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                CreateProblem(StatusCodes.Status409Conflict, "Conflict",
                    conflictEx.Message, correlationId)),

            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                CreateProblem(StatusCodes.Status403Forbidden, "Access Denied",
                    forbiddenEx.Message, correlationId)),

            DomainException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                CreateProblem(StatusCodes.Status422UnprocessableEntity, "Business Rule Violation",
                    domainEx.Message, correlationId)),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                CreateProblem(StatusCodes.Status401Unauthorized, "Unauthorized",
                    "Authentication is required to access this resource.", correlationId)),

            OperationCanceledException => (
                499, // Client Closed Request
                CreateProblem(499, "Request Cancelled",
                    "The request was cancelled by the client.", correlationId)),

            _ => HandleUnexpectedException(exception, correlationId)
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }

    private (int, ProblemDetails) HandleUnexpectedException(Exception exception, string correlationId)
    {
        // Generate a unique error reference for support correlation
        var errorId = Guid.NewGuid().ToString("N")[..12].ToUpper();

        // Log FULL details internally — including stack trace, inner exception, etc.
        _logger.LogError(exception,
            "Unhandled exception. ErrorId={ErrorId} CorrelationId={CorrelationId} ExceptionType={ExceptionType}",
            errorId, correlationId, exception.GetType().Name);

        // Return ZERO internal details to the client (information disclosure prevention)
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please contact support with the error reference.",
            Extensions =
            {
                ["errorId"] = errorId,
                ["correlationId"] = correlationId
            }
        };

        return (StatusCodes.Status500InternalServerError, problem);
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail, string correlationId)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Extensions = { ["correlationId"] = correlationId }
        };
    }

    private static ProblemDetails CreateValidationProblem(ValidationException ex, string correlationId)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred. See the errors property for details.",
            Extensions =
            {
                ["errors"] = errors,
                ["correlationId"] = correlationId
            }
        };
    }
}
