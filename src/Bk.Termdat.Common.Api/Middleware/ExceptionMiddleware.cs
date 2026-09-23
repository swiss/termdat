using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bk.Termdat.Common.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly bool showErrorDetails;

    public ExceptionMiddleware(RequestDelegate next, bool showErrorDetails)
    {
        this.next = next;
        this.showErrorDetails = showErrorDetails;
    }

    public async Task InvokeAsync(HttpContext httpContext, ILogger<ExceptionMiddleware> logger)
    {
        try
        {
            await next(httpContext);
        }
        catch (SearchException ex)
        {
            logger.LogWarning("Search exception: {Message}", ex.Message);
            await BadRequest(httpContext, string.Empty, ex.Message);
        }
        catch (SearchTimeoutException ex)
        {
            logger.LogWarning("Search timeout exception: {Message}", ex.Message);
            await GatewayTimeout(httpContext, ex.Message);
        }
        catch (BusinessValidationException ex)
        {
            logger.LogInformation("Business validation exception: {Message}", ex.Message);
            await BadRequest(httpContext, ex.ModelKey, ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            logger.LogInformation("Not found: {Message}", ex.Message);
            await NotFound(httpContext, ex);
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning("Forbidden: {Message}", ex.Message);
            await httpContext.ForbidAsync();
        }
        catch (ConcurrencyException ex)
        {
            logger.LogWarning("Conflict: {Message}", ex.Message);
            await HandleConcurrencyException(httpContext, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Conflict: {Message}", ex.Message);
            await HandleConcurrencyException(httpContext, ex);
        }
        catch (DbUpdateException ex)
        {
            await HandleDbUpdateExceptionAsync(httpContext, logger, ex);
        }
        catch (CanNotBeChangedException ex)
        {
            logger.LogWarning("CanNotBeChanged exception: {Message}", ex.Message);
            await BadRequest(httpContext, string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Uncaught exception: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleDbUpdateExceptionAsync(HttpContext httpContext, ILogger<ExceptionMiddleware> logger, DbUpdateException ex)
    {
        logger.LogWarning("Db update exception: {Message}", ex.Message);
        if (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            var uniqueConstraintViolation = ex.Entries.Any() && ex.Entries.All(e =>
            {
                var type = e.Entity.GetType();
                return type == typeof(Glossary) || type == typeof(Collection) || type == typeof(Office) || type == typeof(Language) || type == typeof(AdminInfo);
            })
                ? BackendTexts.UniqueConstraintViolationCode
                : BackendTexts.UniqueConstraintViolation;

            await BadRequest(httpContext, string.Empty, uniqueConstraintViolation);
        }
        else
        {
            logger.LogError(ex, "Uncaught db update exception: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task NotFound(HttpContext httpContext, Exception ex)
    {
        var result = new NotFoundObjectResult(ex.Message);
        await result.ExecuteResultAsync(new ActionContext
        {
            HttpContext = httpContext,
            RouteData = httpContext.GetRouteData()
        });
    }

    private static async Task BadRequest(HttpContext httpContext, string modelKey, string message)
    {
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = httpContext.GetRouteData()
        };
        actionContext.ModelState.AddModelError(modelKey, message);

        var apiBehaviorOptions = httpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        var invalidModelStateResponse = apiBehaviorOptions.Value.InvalidModelStateResponseFactory(actionContext);

        await invalidModelStateResponse.ExecuteResultAsync(actionContext);
    }

    private static async Task GatewayTimeout(HttpContext httpContext, string message)
    {
        var result = new ObjectResult(message)
        {
            StatusCode = (int)HttpStatusCode.GatewayTimeout
        };
        await result.ExecuteResultAsync(new ActionContext
        {
            HttpContext = httpContext,
            RouteData = httpContext.GetRouteData()
        });
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = GetProblemDetails(context, exception);
        var result = new ObjectResult(problem)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        await result.ExecuteResultAsync(new ActionContext
        {
            HttpContext = context,
            RouteData = context.GetRouteData()
        });
    }

    private static async Task HandleConcurrencyException(HttpContext httpContext, DbUpdateConcurrencyException exception)
    {
        await ConflictObject(
            httpContext,
            exception.Message);
    }

    private static async Task HandleConcurrencyException(HttpContext httpContext, ConcurrencyException exception)
    {
        await ConflictObject(
            httpContext,
            exception.Message);
    }

    private static async Task ConflictObject(HttpContext httpContext, object conflictObject)
    {
        var result = new ConflictObjectResult(conflictObject);
        await result.ExecuteResultAsync(new ActionContext
        {
            HttpContext = httpContext,
            RouteData = httpContext.GetRouteData()
        });
    }

    private ProblemDetails GetProblemDetails(HttpContext httpContext, Exception exception)
    {
        var problemFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        return showErrorDetails
            ? problemFactory.CreateProblemDetails(
                httpContext,
                (int)HttpStatusCode.InternalServerError,
                exception.Message,
                instance: exception.Source,
                detail: exception.StackTrace
            )
            : problemFactory.CreateProblemDetails(httpContext, (int)HttpStatusCode.InternalServerError);
    }
}
