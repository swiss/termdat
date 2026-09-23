using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bk.Termdat.Common.Api.Middleware;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Bk.Termdat.Common.Api.Tests.Middleware;

[TestFixture]
[SetCulture("de")]
internal class ExceptionMiddlewareTests
{
    private ExceptionMiddleware middleware;

    private RequestDelegate next;
    private readonly ILogger<ExceptionMiddleware> loggerMock = Substitute.For<ILogger<ExceptionMiddleware>>();
    private readonly HttpContext contextMock = Substitute.For<HttpContext>();

    [SetUp]
    public void SetUp()
    {
        next = _ => Task.CompletedTask;
    }

    [TearDown]
    public void TearDown()
    {
        contextMock.ClearSubstitute();
        loggerMock.ClearSubstitute();
    }

    [Test]
    public async Task InvokeOnlyModifiesContextOnException()
    {
        middleware = new ExceptionMiddleware(next, false);
        await middleware.InvokeAsync(contextMock, loggerMock);

        loggerMock.Received(0).Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Test]
    public async Task InvokeCatchesForbiddenException()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationServiceMock);
        serviceProvider.GetService(typeof(ILoggerFactory)).Returns(Substitute.For<ILoggerFactory>());

        contextMock.RequestServices.Returns(serviceProvider);

        next = _ => Task.FromException(new ForbiddenException("SHOULD BE CATCHED"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });

        await authenticationServiceMock.Received(1)
            .ForbidAsync(Arg.Is(contextMock), Arg.Any<string>(), Arg.Any<AuthenticationProperties>());
    }

    [Test]
    public void InvokeCatchesEntityNotFoundException()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var actionResultExecutorMock = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_ => authenticationServiceMock)
                .AddSingleton(actionResultExecutorMock)
                .BuildServiceProvider()
        };

        next = _ => Task.FromException(new EntityNotFoundException("SHOULD BE CATCHED"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(context, loggerMock);
        });
    }

    [Test]
    public void InvokeCatchesSearchException()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var invalidModelStateResponseFactoryCalled = false;
        var apiBehaviorOptionsMock = Substitute.For<IOptions<ApiBehaviorOptions>>();
        apiBehaviorOptionsMock.Value.Returns(new ApiBehaviorOptions
        {
            InvalidModelStateResponseFactory = _ =>
            {
                invalidModelStateResponseFactoryCalled = true;
                return new NoContentResult();
            }
        });

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_ => apiBehaviorOptionsMock)
                .AddSingleton(_ => authenticationServiceMock)
                .BuildServiceProvider()
        };

        next = _ => Task.FromException(new SearchException("SHOULD BE CATCHED"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(context, loggerMock);
        });
        Assert.That(invalidModelStateResponseFactoryCalled, Is.True);
    }

    [Test]
    public void InvokeCatchesSearchTimeoutException()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var actionResultExecutorMock = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_ => authenticationServiceMock)
                .AddSingleton(actionResultExecutorMock)
                .BuildServiceProvider()
        };

        next = _ => Task.FromException(new SearchTimeoutException("SHOULD BE CATCHED"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(context, loggerMock);
        });
    }

    [Test]
    public void InvokeCatchesBusinessValidationException()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var invalidModelStateResponseFactoryCalled = false;
        var apiBehaviorOptionsMock = Substitute.For<IOptions<ApiBehaviorOptions>>();
        apiBehaviorOptionsMock.Value.Returns(new ApiBehaviorOptions
        {
            InvalidModelStateResponseFactory = _ =>
            {
                invalidModelStateResponseFactoryCalled = true;
                return new NoContentResult();
            }
        });

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_ => apiBehaviorOptionsMock)
                .AddSingleton(_ => authenticationServiceMock)
                .BuildServiceProvider()
        };

        next = _ => Task.FromException(new BusinessValidationException(string.Empty, "SHOULD BE CATCHED"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(context, loggerMock);
        });
        Assert.That(invalidModelStateResponseFactoryCalled, Is.True);
    }

    [Test]
    public void InvokeCatchesDbUpdateExceptionForUniqueViolation()
    {
        var authenticationServiceMock = Substitute.For<IAuthenticationService>();
        authenticationServiceMock.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var invalidModelStateResponseFactoryCalled = false;
        var invalidModelStateError = string.Empty;
        var apiBehaviorOptionsMock = Substitute.For<IOptions<ApiBehaviorOptions>>();
        apiBehaviorOptionsMock.Value.Returns(new ApiBehaviorOptions
        {
            InvalidModelStateResponseFactory = actionContext =>
            {
                invalidModelStateError = actionContext.ModelState.Values.First().Errors.First().ErrorMessage;
                invalidModelStateResponseFactoryCalled = true;
                return new NoContentResult();
            }
        });

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_ => apiBehaviorOptionsMock)
                .AddSingleton(_ => authenticationServiceMock)
                .BuildServiceProvider(),
        };
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/";

        next = _ => Task.FromException(new DbUpdateException("SHOULD BE CATCHED", new PostgresException("PostgresSql Exception", "hups", "really?", PostgresErrorCodes.UniqueViolation)));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(context, loggerMock);
        });
        Assert.That(invalidModelStateResponseFactoryCalled, Is.True);
        Assert.That(invalidModelStateError, Is.EqualTo(BackendTexts.UniqueConstraintViolation));
    }

    [TestCase(PostgresErrorCodes.ActiveSqlTransaction)]
    [TestCase(PostgresErrorCodes.CannotConnectNow)]
    [TestCase(PostgresErrorCodes.ConnectionFailure)]
    [TestCase(PostgresErrorCodes.DiskFull)]
    [TestCase(PostgresErrorCodes.DivisionByZero)]
    public async Task InvokeCatchesDbUpdateExceptionForOtherErrors(string errorCode)
    {
        var actionResultExecutorMock = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var details = Substitute.For<ProblemDetails>();

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        problemDetailsFactory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(details);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IActionResultExecutor<ObjectResult>)).Returns(actionResultExecutorMock);
        serviceProvider.GetService(typeof(ProblemDetailsFactory)).Returns(problemDetailsFactory);

        contextMock.RequestServices.Returns(serviceProvider);
        contextMock.Request.RouteValues = new RouteValueDictionary { { "test", "test" } };

        next = _ =>
        {
            var npgsqlException = new PostgresException("PostgresSql Exception", "hups", "really?", errorCode);
            var dbUpdateException = new DbUpdateException("SHOULD BE CATCHED", npgsqlException);
            return Task.FromException(dbUpdateException);
        };
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });

        problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Is(contextMock),
            Arg.Is((int)HttpStatusCode.InternalServerError)
        );
        await actionResultExecutorMock.Received(1).ExecuteAsync(
            Arg.Is<ActionContext>(x => x.HttpContext == contextMock),
            Arg.Is<ObjectResult>(x => x.StatusCode == (int)HttpStatusCode.InternalServerError && x.Value == details)
        );
    }

    [Test]
    public void InvokeCatchesDbUpdateConcurrencyException()
    {
        var actionResultExecutorMock1 = Substitute.For<IActionResultExecutor<ConflictObjectResult>>();
        actionResultExecutorMock1.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ConflictObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);
        var actionResultExecutorMock2 = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock2.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var authenticationService = Substitute.For<IAuthenticationService>();
        authenticationService.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IActionResultExecutor<ConflictObjectResult>)).Returns(actionResultExecutorMock1);
        serviceProvider.GetService(typeof(IActionResultExecutor<ObjectResult>)).Returns(actionResultExecutorMock2);

        contextMock.RequestServices.Returns(serviceProvider);
        contextMock.Request.RouteValues = new RouteValueDictionary { { "test", "test" } };

        var entryMock = Substitute.For<IUpdateEntry>();
        var entriesMock = new List<IUpdateEntry> { entryMock };
        var exception = new DbUpdateConcurrencyException(string.Empty, entriesMock);

        next = _ => Task.FromException(exception);
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });
    }

    [Test]
    public void InvokeCatchesConcurrencyException()
    {
        var actionResultExecutorMock1 = Substitute.For<IActionResultExecutor<ConflictObjectResult>>();
        actionResultExecutorMock1.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ConflictObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);
        var actionResultExecutorMock2 = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock2.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var authenticationService = Substitute.For<IAuthenticationService>();
        authenticationService.ForbidAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IActionResultExecutor<ConflictObjectResult>)).Returns(actionResultExecutorMock1);
        serviceProvider.GetService(typeof(IActionResultExecutor<ObjectResult>)).Returns(actionResultExecutorMock2);

        contextMock.RequestServices.Returns(serviceProvider);
        contextMock.Request.RouteValues = new RouteValueDictionary { { "test", "test" } };

        var exception = new ConcurrencyException(new { }, new { });

        next = _ => Task.FromException(exception);
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });
    }

    [Test]
    public async Task InvokeCatchesGeneralExceptions()
    {
        var actionResultExecutorMock = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var details = Substitute.For<ProblemDetails>();

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        problemDetailsFactory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(details);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IActionResultExecutor<ObjectResult>)).Returns(actionResultExecutorMock);
        serviceProvider.GetService(typeof(ProblemDetailsFactory)).Returns(problemDetailsFactory);

        contextMock.RequestServices.Returns(serviceProvider);
        contextMock.Request.RouteValues = new RouteValueDictionary { { "test", "test" } };

        next = _ => Task.FromException(new Exception("SHOULD CATCH THIS"));
        middleware = new ExceptionMiddleware(next, false);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });

        problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Is(contextMock),
            Arg.Is((int)HttpStatusCode.InternalServerError)
        );
        await actionResultExecutorMock.Received(1).ExecuteAsync(
            Arg.Is<ActionContext>(x => x.HttpContext == contextMock),
            Arg.Is<ObjectResult>(x => x.StatusCode == (int)HttpStatusCode.InternalServerError && x.Value == details)
        );
    }

    [Test]
    public async Task ShowDetailsControlsErrorInformationCorrectly()
    {
        var actionResultExecutorMock = Substitute.For<IActionResultExecutor<ObjectResult>>();
        actionResultExecutorMock.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
            .ReturnsForAnyArgs(Task.CompletedTask);

        var details = Substitute.For<ProblemDetails>();

        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        problemDetailsFactory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(details);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IActionResultExecutor<ObjectResult>)).Returns(actionResultExecutorMock);
        serviceProvider.GetService(typeof(ProblemDetailsFactory)).Returns(problemDetailsFactory);

        contextMock.RequestServices.Returns(serviceProvider);
        contextMock.Request.RouteValues = new RouteValueDictionary { { "test", "test" } };

        next = _ => Task.FromException(new Exception("SHOULD CATCH THIS WITH DETAILS"));
        middleware = new ExceptionMiddleware(next, true);

        Assert.DoesNotThrowAsync(async () =>
        {
            await middleware.InvokeAsync(contextMock, loggerMock);
        });

        problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Is(contextMock),
            Arg.Is((int)HttpStatusCode.InternalServerError),
            Arg.Is("SHOULD CATCH THIS WITH DETAILS"),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()
        );
        await actionResultExecutorMock.Received(1).ExecuteAsync(
            Arg.Is<ActionContext>(x => x.HttpContext == contextMock),
            Arg.Is<ObjectResult>(x => x.StatusCode == (int)HttpStatusCode.InternalServerError && x.Value == details)
        );
    }
}
