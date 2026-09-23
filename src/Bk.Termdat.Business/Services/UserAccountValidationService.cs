using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Services;

public class UserAccountValidationService : IUserAccountValidationService
{
    private readonly ICollectionRepository collectionRepository;
    private readonly IGlossaryRepository glossaryRepository;

    public UserAccountValidationService(
        ICollectionRepository collectionRepository,
        IGlossaryRepository glossaryRepository
    )
    {
        this.collectionRepository = collectionRepository;
        this.glossaryRepository = glossaryRepository;
    }

    public async Task<ICollection<Glossary>> ValidateGlossaries(IUserAccountModification userAccountModification)
    {
        if (userAccountModification.AuthorizedGlossaryIds == null || !userAccountModification.AuthorizedGlossaryIds.Any())
        {
            return Array.Empty<Glossary>();
        }

        var requestedGlossaries = (await glossaryRepository.GetByIds(userAccountModification.AuthorizedGlossaryIds))
            .ToList();

        var requestedGlossaryIds = userAccountModification.AuthorizedGlossaryIds.OrderBy(id => id).ToArray();
        var loadedGlossaryIds = requestedGlossaries.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedGlossaryIds.SequenceEqual(loadedGlossaryIds))
        {
            throw new BusinessValidationException(
                nameof(userAccountModification.AuthorizedGlossaryIds),
                string.Format(BackendTexts.EntryGlossariesNotFound,
                    string.Join(",", requestedGlossaryIds)));
        }

        return requestedGlossaries;
    }

    public async Task<ICollection<Collection>> ValidateCollections(IUserAccountModification userAccountModification)
    {
        if (userAccountModification.AuthorizedCollectionIds == null || !userAccountModification.AuthorizedCollectionIds.Any())
        {
            return Array.Empty<Collection>();
        }

        var requestedCollections = (await collectionRepository.GetByIds(userAccountModification.AuthorizedCollectionIds))
            .ToList();

        var requestedCollectionIds = userAccountModification.AuthorizedCollectionIds.OrderBy(id => id).ToArray();
        var loadedCollectionIds = requestedCollections.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedCollectionIds.SequenceEqual(loadedCollectionIds))
        {
            throw new BusinessValidationException(
                nameof(userAccountModification.AuthorizedCollectionIds),
                string.Format(BackendTexts.CollectionNotFound,
                    string.Join(",", requestedCollectionIds)));
        }

        return requestedCollections;
    }
}
