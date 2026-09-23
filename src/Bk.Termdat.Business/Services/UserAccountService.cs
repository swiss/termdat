using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class UserAccountService : IUserAccountService
{
    private readonly IMapper mapper;
    private readonly IUserAccountRepository userAccountRepository;
    private readonly IIdentityService identityService;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<UserAccountService> logger;
    private readonly IUserAccountValidationService userAccountValidationService;
    private readonly AuthorizationOptions options;

    private const string System = nameof(System);

    public UserAccountService(
        IMapper mapper,
        IUserAccountRepository userAccountRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILogger<UserAccountService> logger,
        IUserAccountValidationService userAccountValidationService,
        IOptions<AuthorizationOptions> options)
    {
        this.mapper = mapper;
        this.userAccountRepository = userAccountRepository;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.userAccountValidationService = userAccountValidationService;
        this.options = options.Value;
    }

    public async Task<IEnumerable<UserAccountSummary>> GetAll()
    {
        var userAccounts = await userAccountRepository.GetAll();
        return mapper.Map<IEnumerable<UserAccountSummary>>(userAccounts);
    }

    public async Task<UserAccountDetail> CreateOrUpdate()
    {
        var currentEmail = identityService.GetCurrentEmail();
        var currentFirstName = identityService.GetCurrentFirstName();
        var currentLastName = identityService.GetCurrentLastName();
        var role = identityService.GetCurrentRole();

        logger.LogDebug(
            "User information from claims, email='{CurrentEmail}' fistName='{CurrentFirstName}' lastName='{CurrentLastName}'",
            currentEmail, currentFirstName, currentLastName);

        var account = await userAccountRepository.GetDetailByEmailForUpdate(currentEmail);
        if (account is null)
        {
            logger.LogDebug("User with email='{CurrentEmail}' not found, adding...", currentEmail);

            var newUser = new UserAccount
            {
                Created = DateTime.UtcNow,
                CreatedBy = System,
                Modified = DateTime.UtcNow,
                ModifiedBy = System,
                Email = currentEmail,
                LastName = currentLastName,
                FirstName = currentFirstName,
                Role = role,
                LastLogin = DateTime.UtcNow
            };
            await userAccountRepository.Create(newUser);
        }
        else
        {
            logger.LogDebug("User with email='{CurrentEmail}' found, updating...", currentEmail);

            var update = new UserAccount
            {
                Id = account.Id,
                Created = account.Created,
                CreatedBy = account.CreatedBy,
                Modified = DateTime.UtcNow,
                ModifiedBy = System,
                Email = currentEmail,
                LastName = currentLastName,
                FirstName = currentFirstName,
                Role = role,
                LastAction = account.LastAction,
                LastLogin = DateTime.UtcNow,
            };
            userAccountRepository.Update(account, update);
        }

        await unitOfWork.SaveChangesAsync();

        account = await userAccountRepository.GetDetailByEmail(currentEmail);

        return mapper.Map<UserAccountDetail>(account);
    }

    public async Task<UserAccountDetail> GetDetail(int id)
    {
        var userAccount = await userAccountRepository.GetDetail(id);
        if (userAccount == null)
        {
            throw new EntityNotFoundException(string.Format(BackendTexts.UserAccountNotFound, id));
        }
        var mappedUserAccount = mapper.Map<UserAccountDetail>(userAccount);

        return mappedUserAccount;
    }

    public async Task<IEnumerable<UserAccountDetail>> GetAllDetails()
    {
        var userAccounts = await userAccountRepository.GetAllDetailed();
        return mapper.Map<IEnumerable<UserAccountDetail>>(userAccounts);
    }

    public async Task UpdateLastAction(string email)
    {
        var existingUserAccount = await userAccountRepository.GetByEmail(email);

        if (existingUserAccount == null)
        {
            throw new EntityNotFoundException($"User with email '{email}' was not found");
        }

        existingUserAccount.LastAction = DateTime.UtcNow;
    }

    public async Task Update(UserAccountModification userAccountModification)
    {
        var databaseEntry = await userAccountRepository.GetDetailByIdForUpdate(userAccountModification.Id);
        if (databaseEntry is null)
        {
            throw new EntityNotFoundException(BackendTexts.EntryNotFound);
        }

        if (databaseEntry.Role != options.Writer && databaseEntry.Role != options.SuperReader)
        {
            throw new BusinessValidationException(nameof(databaseEntry.Role), BackendTexts.UserAccountUpdateWriterOrSuperReaderOnlyError);
        }

        var clientEntry = mapper.Map<UserAccount>(userAccountModification);

        clientEntry.AuthorizedCollections = await userAccountValidationService.ValidateCollections(userAccountModification);
        clientEntry.AuthorizedGlossaries = databaseEntry.Role == options.Writer ? await userAccountValidationService.ValidateGlossaries(userAccountModification) : [];
        clientEntry.FirstName = databaseEntry.FirstName;
        clientEntry.LastName = databaseEntry.LastName;
        clientEntry.Role = databaseEntry.Role;
        clientEntry.LastAction = databaseEntry.LastAction;
        clientEntry.LastLogin = databaseEntry.LastLogin;
        clientEntry.Email = databaseEntry.Email;
        clientEntry.Created = databaseEntry.Created;
        clientEntry.CreatedBy = databaseEntry.CreatedBy;
        clientEntry.Modified = DateTime.UtcNow;
        clientEntry.ModifiedBy = identityService.GetCurrentEmail();

        userAccountRepository.UpdateWithRelations(databaseEntry, clientEntry);

        await unitOfWork.SaveChangesAsync();
    }
}
