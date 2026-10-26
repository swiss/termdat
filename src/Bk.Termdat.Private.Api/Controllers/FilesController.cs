using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bk.Termdat.Business.Extensions;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.Private.Api.HostedServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IAuthorizationService = Bk.Termdat.Business.Services.Interfaces.IAuthorizationService;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IAuthorizationService authorizationService;
    private readonly IBackgroundTaskQueue backgroundTaskQueue;
    private readonly IFileImportService fileImportService;
    private readonly IIdentityService identityService;
    private readonly ILogger<FilesController> logger;

    public FilesController(IFileImportService fileImportService, IBackgroundTaskQueue backgroundTaskQueue,
        ILogger<FilesController> logger, IIdentityService identityService, IAuthorizationService authorizationService)
    {
        this.fileImportService = fileImportService;
        this.backgroundTaskQueue = backgroundTaskQueue;
        this.logger = logger;
        this.identityService = identityService;
        this.authorizationService = authorizationService;
    }

    [HttpPost("entries:import")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    [RequestSizeLimit(51L * 1024 * 1024)] // 51MB: 50MB for the file, 1MB spare for the rest of the request data
    public async Task<ActionResult> Import([FromForm][Required] FileImportRequest importRequest)
    {
        ArgumentNullException.ThrowIfNull(importRequest);

        try
        {
            var currentUserEmail = identityService.GetCurrentEmail();
            if (string.IsNullOrWhiteSpace(currentUserEmail))
            {
                ModelState.AddModelError("UnexpectedError", "Current user email is not available");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var (isValid, validationErrors, document, fileType) =
                await fileImportService.IsFileValid(importRequest.File);

            if (!isValid)
            {
                foreach (var validationError in validationErrors)
                {
                    ModelState.AddModelError(validationError.Key, validationError.Value);
                }

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var importJobLabel = $"{fileType.ToUpperInvariant()}-Import-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
            var staticIdentityService = identityService.ConvertToStaticIdentityService();
            var staticAuthorizationService = authorizationService.ConvertToStaticAuthorizationService();

            await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (_, serviceProvider) =>
            {
                using var scope = serviceProvider.CreateScope();
                var workItemLogger = scope.ServiceProvider.GetRequiredService<ILogger<QueuedBackgroundService>>();

                var workItemName = $"{nameof(IFileImportService)}.{nameof(IFileImportService.ImportDocument)}";

                if (workItemLogger.IsEnabled(LogLevel.Information))
                {
                    workItemLogger.LogInformation(
                        "Queued background work item {WorkItemName} - {ImportJobLabel}  is starting", workItemName,
                        importJobLabel);
                }

                // Note:
                // Override the default implementation of the authorization service with an always-true version to suppress any HttpContext-based role checking during the import process, which runs in the background.
                // This should not be a problem as long as this import job is an admin role task.
                // Be aware that this only works as long as the involved services are scoped!
                var authorizationServiceProvider =
                    scope.ServiceProvider.GetService<OverrideableAuthorizationServiceProvider>();
                authorizationServiceProvider.Provider = staticAuthorizationService;

                // Same for the Identity Service to bypass HttpContext based claims access.
                var identityServiceProvider = scope.ServiceProvider.GetService<OverrideableIdentityServiceProvider>();
                identityServiceProvider.Provider = staticIdentityService;

                var importService = scope.ServiceProvider.GetService<IFileImportService>();

                await importService.ImportDocument(document, fileType, importJobLabel, currentUserEmail,
                    importRequest.EntryOffice, importRequest.EntryCollection,
                    importRequest.Status, importRequest.Reliability,
                    importRequest.SubjectIds, importRequest.DomainIds, importRequest.GlossaryIds,
                    importRequest.AdminInfos, importRequest.Comment);
                if (workItemLogger.IsEnabled(LogLevel.Information))
                {
                    workItemLogger.LogInformation("Queued background work item {WorkItemName} - {ImportJobLabel} ended",
                        workItemName, importJobLabel);
                }
            });

            return Ok(new FileImportResponse { AdminInfoCode = importJobLabel });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error during import task creation attempt");

            ModelState.AddModelError("UnexpectedError", "Unexpected error during import task creation attempt");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }
    }
}
