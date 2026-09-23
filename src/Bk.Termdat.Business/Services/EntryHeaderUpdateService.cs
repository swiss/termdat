using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class EntryHeaderUpdateService : IEntryHeaderUpdateService
{
    private readonly IEntryRepository entryRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ISearchQueueClient searchQueueClient;
    private readonly IMapper mapper;
    private readonly IEntryValidationService entryValidationService;
    private readonly IUserAccountService userAccountService;
    private readonly IIdentityService identityService;
    private readonly IAdminInfoService adminInfoService;
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<EntryHeaderUpdateService> logger;

    public EntryHeaderUpdateService(IEntryValidationService entryValidationService,
        IUserAccountService userAccountService,
        IIdentityService identityService,
        IAdminInfoService adminInfoService,
        IUnitOfWork unitOfWork,
        IEntryRepository entryRepository,
        ISearchQueueClient searchQueueClient,
        IAuthorizationService authorizationService,
        IMapper mapper,
        ILogger<EntryHeaderUpdateService> logger)
    {
        this.entryValidationService = entryValidationService;
        this.userAccountService = userAccountService;
        this.identityService = identityService;
        this.adminInfoService = adminInfoService;
        this.searchQueueClient = searchQueueClient;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
        this.entryRepository = entryRepository;
        this.authorizationService = authorizationService;
        this.logger = logger;
    }

    public async Task<EntryMultipleUpdateResult> UpdateMandatoryHeaderFields(EntryHeaderUpdate update)
    {
        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);
        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        if (!authorizationService.IsAdmin())
        {
            if (update.CollectionId is int collectionId)
            {
                var authorizedCollectionIds = authorizationService.GetAuthorizedCollectionIds();

                if (!authorizedCollectionIds.Contains(collectionId))
                {
                    throw new ForbiddenException(BackendTexts.Unauthorized);
                }
            }
        }

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        var dataWasChanged = false;

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            if ((update.CollectionId.HasValue && databaseEntry.CollectionId != update.CollectionId)
                || (update.OfficeId.HasValue && databaseEntry.OfficeId != update.OfficeId)
                || (update.Reliability.HasValue && (int)databaseEntry.Reliability != (int)update.Reliability.Value)
                || (update.Status.HasValue && (int)databaseEntry.Status != (int)update.Status)
                || (update.SubjectIds is not null && !databaseEntry.Subjects.Select(s => s.Id).OrderBy(id => id).SequenceEqual(update.SubjectIds.OrderBy(id => id)))
                || (update.DomainIds is not null && !databaseEntry.Domains.Select(s => s.Id).OrderBy(id => id).SequenceEqual(update.DomainIds.OrderBy(id => id))))
            {
                dataWasChanged = true;
            }
            var clientEntry = await CreateEntryHeaderUpdate(update, databaseEntry);

            if (!update.ReplaceSubjects)
            {
                clientEntry.Subjects = databaseEntry.Subjects.Union(clientEntry.Subjects).ToList();
                clientEntry.Domains = databaseEntry.Domains.Union(clientEntry.Domains).ToList();
            }

            clientEntry.ConcurrencyToken = databaseEntry.ConcurrencyToken;
            clientEntry.Identification = databaseEntry.Identification;
            clientEntry.Comment = databaseEntry.Comment;
            clientEntry.AdminInfo = databaseEntry.AdminInfo;
            clientEntry.Deleted = databaseEntry.Deleted;
            clientEntry.ConceptSystemPosition = databaseEntry.ConceptSystemPosition;
            clientEntry.ConceptSystemPositionId = databaseEntry.ConceptSystemPositionId;
            clientEntry.Picture = databaseEntry.Picture;

            SetChangeTrackingFields(clientEntry, databaseEntry);
            entryRepository.UpdateMandatoryHeaderFields(databaseEntry, clientEntry);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = dataWasChanged,
        };
    }

    public async Task<EntryMultipleUpdateResult> UpdateGlossaries(EntryChipFieldUpdate update)
    {
        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);
        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        if (!authorizationService.IsAdmin())
        {
            var authorizedGlossaryIds = authorizationService.GetAuthorizedGlossaryIds();

            var ids = ParseCodesToIds(update.Codes);

            if (!ids.All(id => authorizedGlossaryIds.Contains(id)))
            {
                throw new ForbiddenException(BackendTexts.Unauthorized);

            }
        }

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var clientEntry = await CreateEntryUpdateGlossary(databaseEntry.Id, update, databaseEntry);
            SetChangeTrackingFields(clientEntry, databaseEntry);
            entryRepository.UpdateGlossaries(databaseEntry, clientEntry);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        var count = await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = count > update.Ids.Count() + 1
        };
    }

    public async Task<EntryMultipleUpdateResult> UpdateComment(EntryCommentUpdate update)
    {
        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);

        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        var dataWasChanged = false;

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            if (update.Action == MultiEditAction.Add || (update.Action == MultiEditAction.Remove && !string.IsNullOrEmpty(databaseEntry.Comment))
                || (update.Action == MultiEditAction.Set && databaseEntry.Comment != update.Comment))
            {
                dataWasChanged = true;
            }

            var clientEntry = CreateEntryUpdateComment(update, databaseEntry);
            SetChangeTrackingFields(clientEntry, databaseEntry);
            entryRepository.UpdateWithoutRelations(databaseEntry, clientEntry);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = dataWasChanged,
        };
    }

    public async Task<EntryMultipleUpdateResult> UpdateConceptSystemPosition(EntryConceptSystemPositionUpdate updateDto)
    {
        if (updateDto.Action == MultiEditAction.Add)
        {
            throw new BusinessValidationException(nameof(Entry.ConceptSystemPositionId), "It is not possible to 'add' the position in the concept system during batch editing.");
        }

        var databaseEntries = await entryRepository.GetByIdsForUpdate(updateDto.Ids);

        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        var dataWasChanged = (updateDto.Action == MultiEditAction.Remove && databaseEntries.Any(p => p.Locked is null && p.ConceptSystemPositionId.HasValue))
            || (updateDto.Action == MultiEditAction.Set && databaseEntries.Any(p => p.Locked is null && p.ConceptSystemPositionId != updateDto.ConceptSystemPositionId));

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            databaseEntry.ConceptSystemPositionId = updateDto.Action == MultiEditAction.Set ? updateDto.ConceptSystemPositionId : null;
            databaseEntry.Modified = DateTime.UtcNow;
            databaseEntry.ModifiedBy = identityService.GetCurrentEmail();
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(databaseEntries.Where(y => y.Locked is null).Select(e => e.Id));

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = dataWasChanged,
        };
    }

    public async Task<EntrySourceUpdateResult> UpdateSource(EntrySourceUpdate update)
    {
        if (update.Action == MultiEditAction.Remove)
        {
            throw new BusinessValidationException(nameof(update.Action), "It is not possible to 'remove' sources during batch editing.");
        }

        if (string.IsNullOrWhiteSpace(update.UpdateValue))
        {
            throw new BusinessValidationException(nameof(update.UpdateValue), "Empty source is not allowed.");
        }

        if (!update.TargetFields.Any())
        {
            throw new BusinessValidationException(nameof(update.TargetFields), "Missing target field.");
        }

        if (!update.TargetFields.All(s => s is EntrySearchAndReplaceUpdate.DefinitionSource or
            EntrySearchAndReplaceUpdate.NoteSource or
            EntrySearchAndReplaceUpdate.ContextSource or
            EntrySearchAndReplaceUpdate.AbbreviationSource or
            EntrySearchAndReplaceUpdate.TerminusSource or
            EntrySearchAndReplaceUpdate.NameSource or
            EntrySearchAndReplaceUpdate.MetadataSource or
            EntrySearchAndReplaceUpdate.PhraseologySource))
        {
            throw new BusinessValidationException(nameof(update.TargetFields), "Invalid target field.");
        }

        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);

        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        var dataWasChanged = false;
        var changedEntryIds = new List<int>();
        var notFullyUpdatedEntryIds = new List<int>();
        var userName = identityService.GetCurrentEmail();

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var entryWasChanged = false;
            var notFullyUpdatedEntry = false;
            foreach (var item in databaseEntry.Translations.Where(y => update.LanguageIds.Contains(y.LanguageId)))
            {
                foreach (var targetField in update.TargetFields)
                {
                    if (TryUpdateSourceField(item, targetField, update.UpdateValue, update.Action, out var notFully))
                    {
                        entryWasChanged = true;
                    }

                    notFullyUpdatedEntry = notFullyUpdatedEntry || notFully;
                }
            }
            if (entryWasChanged)
            {
                changedEntryIds.Add(databaseEntry.Id);
                dataWasChanged = true;
                databaseEntry.Modified = DateTime.UtcNow;
                databaseEntry.ModifiedBy = userName;
            }
            if (notFullyUpdatedEntry)
            {
                notFullyUpdatedEntryIds.Add(databaseEntry.Id);
            }
        }

        await userAccountService.UpdateLastAction(userName);

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(changedEntryIds);

        return new EntrySourceUpdateResult()
        {
            NotFullyUpdatedEntryIds = notFullyUpdatedEntryIds,
            LockInfos = lockInfo.ToList(),
            DataWasChanged = dataWasChanged,
        };
    }

    public async Task<EntryContentMoveResult> MoveContent(EntryContentMove update)
    {
        if (string.IsNullOrWhiteSpace(update.SourceField))
        {
            throw new BusinessValidationException(nameof(update.SourceField), "Empty source field is not allowed.");
        }

        if (string.IsNullOrWhiteSpace(update.DestinationField))
        {
            throw new BusinessValidationException(nameof(update.DestinationField), "Empty destination field is not allowed.");
        }

        if (update.DestinationField == update.SourceField)
        {
            throw new BusinessValidationException(nameof(update.DestinationField), $"The source and destination fields cannot be the same: {update.DestinationField}");
        }

        if (!update.LanguageIds.Any())
        {
            throw new BusinessValidationException(nameof(update.LanguageIds), "Empty languages are not allowed.");
        }

        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);

        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        var dataWasChanged = false;
        var nonChangedEntryIds = new List<int>();
        var userName = identityService.GetCurrentEmail();

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var relevantTranslations = databaseEntry.Translations
                .Where(y => update.LanguageIds.Contains(y.LanguageId));

            if (!update.ForAllSynonyms)
            {
                relevantTranslations = relevantTranslations.Where(t => t.Sequence == 1);
            }

            var allDestinationFieldsAreEmpty = relevantTranslations.All(item => string.IsNullOrWhiteSpace(GetFieldValue(item, update.DestinationField)));
            var allSourceFieldsAreNonEmpty = relevantTranslations.All(item => !string.IsNullOrWhiteSpace(GetFieldValue(item, update.SourceField)));
            var allChangesSupported = relevantTranslations.All(item => IsSupported(item, update.SourceField, update.DestinationField));

            logger.LogDebug("Move content prerequisite checks, allDestinationFieldsAreEmpty: {allDestinationFieldsAreEmpty}, allSourceFieldsAreNonEmpty: {allSourceFieldsAreNonEmpty}, allChangesSupported: {allChangesSupported}",
                allDestinationFieldsAreEmpty, allSourceFieldsAreNonEmpty, allChangesSupported);

            if (!allDestinationFieldsAreEmpty || !allSourceFieldsAreNonEmpty || !allChangesSupported)
            {
                nonChangedEntryIds.Add(databaseEntry.Id);
                continue;
            }

            foreach (var item in relevantTranslations)
            {
                var oldValue = GetFieldValue(item, update.SourceField);
                var oldSource = GetSourceValueForField(item, update.SourceField);
                var newValue = GetFieldValue(item, update.DestinationField);
                var newSource = GetSourceValueForField(item, update.DestinationField);

                SetFieldValue(item, update.DestinationField, oldValue);
                SetSourceValueForField(item, update.DestinationField, oldSource);
                SetFieldValue(item, update.SourceField, string.Empty);
                SetSourceValueForField(item, update.SourceField, string.Empty);

                dataWasChanged = true;
                databaseEntry.Modified = DateTime.UtcNow;
                databaseEntry.ModifiedBy = userName;
            }
        }
        await userAccountService.UpdateLastAction(userName);

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(databaseEntries.Where(x => x.Locked is null).Select(y => y.Id).Except(nonChangedEntryIds));

        return new EntryContentMoveResult()
        {
            NonAffectedEntryIds = nonChangedEntryIds,
            LockInfos = lockInfo.ToList(),
            DataWasChanged = dataWasChanged,
        };
    }

    public async Task<EntryMultipleUpdateResult> UpdateAdminInfos(EntryChipFieldInputUpdate update)
    {
        // Add new AdminInfos and save them in masterdata first
        foreach (var newAdminInfo in update.Codes.Where(a => a.Id == null))
        {
            var adminInfoDetail = await adminInfoService.GetOrCreateAdminInfos(newAdminInfo);
            newAdminInfo.Id = adminInfoDetail.Id;
        }

        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);
        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var clientEntry = await CreateEntryUpdateAdminInfo(databaseEntry.Id, update, databaseEntry);
            SetChangeTrackingFields(clientEntry, databaseEntry);
            entryRepository.UpdateAdminInfos(databaseEntry, clientEntry);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        var count = await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = count > update.Ids.Count() + 1,
        };
    }

    public async Task<EntryMultipleUpdateResult> UpdateRelatedCollections(EntryChipFieldUpdate update)
    {
        var databaseEntries = await entryRepository.GetByIdsForUpdate(update.Ids);
        AssertPermission(databaseEntries);
        AssertNoDeletedEntries(databaseEntries);

        if (!authorizationService.IsAdmin())
        {
            var authorizedCollectionIds = authorizationService.GetAuthorizedCollectionIds();

            var ids = ParseCodesToIds(update.Codes);

            if (!ids.All(id => authorizedCollectionIds.Contains(id)))
            {
                throw new ForbiddenException(BackendTexts.Unauthorized);

            }
        }
        var lockInfo = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy });

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var clientEntry = await CreateEntryUpdateRelatedCollection(databaseEntry.Id, update, databaseEntry);
            SetChangeTrackingFields(clientEntry, databaseEntry);
            entryRepository.UpdateRelatedCollections(databaseEntry, clientEntry);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        var count = await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            LockInfos = lockInfo.ToList(),
            DataWasChanged = count > update.Ids.Count() + 1
        };
    }

    private bool IsSupported(EntryLanguage item, string sourceField, string destinationField)
    {
        static bool HasAny(params string[] fields) => fields.Any(f => !string.IsNullOrWhiteSpace(f));

        if (destinationField is EntrySearchAndReplaceUpdate.Terminus
            or EntrySearchAndReplaceUpdate.Name
            or EntrySearchAndReplaceUpdate.Abbreviation
            or EntrySearchAndReplaceUpdate.Phraseology)
        {
            return true;
        }

        return sourceField switch
        {
            EntrySearchAndReplaceUpdate.Terminus
                => HasAny(item.Name, item.Abbreviation, item.Phraseology),

            EntrySearchAndReplaceUpdate.Name
                => HasAny(item.Terminus, item.Abbreviation, item.Phraseology),

            EntrySearchAndReplaceUpdate.Abbreviation
                => HasAny(item.Terminus, item.Name, item.Phraseology),

            EntrySearchAndReplaceUpdate.Phraseology
                => HasAny(item.Terminus, item.Name, item.Abbreviation),

            _ => false
        };
    }

    private void AssertNoDeletedEntries(IEnumerable<Entry> entryList)
    {
        if (entryList.Any(y => y.Deleted))
        {
            throw new CanNotBeChangedException(string.Format(BackendTexts.EntryDeletedCanNotBeEdited, entryList.FirstOrDefault(y => y.Deleted)!.Id));
        }
    }

    private void AssertPermission(IEnumerable<Entry> entryList)
    {
        if (entryList.Any(y => !authorizationService.CanEdit(y)))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }
    }

    private async Task QueueIndexUpdate(IEnumerable<int> updateIds)
    {
        var entriesToUpdate = await entryRepository.GetByIdsForIndex(updateIds);
        await searchQueueClient.QueueIndexUpdate(entriesToUpdate);
    }

    private async Task<Entry> CreateEntryUpdateAdminInfo(int id, EntryChipFieldInputUpdate entryChipFieldUpdate, Entry databaseEntry)
    {
        var mappedAdminInfos = mapper.Map<IEnumerable<AdminInfoSummary>>(databaseEntry.AdminInfos).ToList();

        var entryUpdate = new EntryUpdate
        {
            Id = id,
            CollectionId = databaseEntry.CollectionId,
            OfficeId = databaseEntry.OfficeId,
            AdminInfos = mappedAdminInfos
        };

        switch (entryChipFieldUpdate.Action)
        {
            case MultiEditAction.Add:
                var adminInfoList = entryChipFieldUpdate.Codes.ToList();
                entryUpdate.AdminInfos = adminInfoList.Union(entryUpdate.AdminInfos).ToList();
                break;
            case MultiEditAction.Set:
                entryUpdate.AdminInfos = entryChipFieldUpdate.Codes.ToList();
                break;
            case MultiEditAction.Remove:
                entryUpdate.AdminInfos = null;
                break;
            default:
                throw new InvalidEnumArgumentException("Please provide implementation", (int)entryChipFieldUpdate.Action, typeof(MultiEditAction));
        }

        var clientEntry = mapper.Map<Entry>(entryUpdate);
        SetPropertiesFromDbEntry(clientEntry, databaseEntry);
        clientEntry.AdminInfos = await entryValidationService.ValidateAdminInfos(entryUpdate);
        return clientEntry;
    }

    private static void SetPropertiesFromDbEntry(Entry clientEntry, Entry databaseEntry, bool includeComment = true)
    {
        if (includeComment)
        {
            clientEntry.Comment = databaseEntry.Comment;
        }

        clientEntry.ConcurrencyToken = databaseEntry.ConcurrencyToken;
        clientEntry.Status = databaseEntry.Status;
        clientEntry.Reliability = databaseEntry.Reliability;
        clientEntry.Identification = databaseEntry.Identification;
        clientEntry.AdminInfo = databaseEntry.AdminInfo;
        clientEntry.Deleted = databaseEntry.Deleted;
        clientEntry.Office = databaseEntry.Office;
        clientEntry.OfficeId = databaseEntry.OfficeId;
        clientEntry.Collection = databaseEntry.Collection;
        clientEntry.CollectionId = databaseEntry.CollectionId;
        clientEntry.ConceptSystemPosition = databaseEntry.ConceptSystemPosition;
        clientEntry.ConceptSystemPositionId = databaseEntry.ConceptSystemPositionId;
        clientEntry.Picture = databaseEntry.Picture;
    }

    private async Task<Entry> CreateEntryUpdateGlossary(int id, EntryChipFieldUpdate entryChipFieldUpdate, Entry databaseEntry)
    {
        var entryUpdate = new EntryUpdate
        {
            Id = id,
            GlossaryIds = databaseEntry.Glossaries.Select(y => y.Id).ToList()
        };
        entryUpdate = UpdateGlossaryIds(entryChipFieldUpdate, entryUpdate);

        var clientEntry = mapper.Map<Entry>(entryUpdate);
        SetPropertiesFromDbEntry(clientEntry, databaseEntry);
        clientEntry.Glossaries = await entryValidationService.ValidateGlossaries(entryUpdate);
        return clientEntry;
    }

    private static EntryUpdate UpdateGlossaryIds(EntryChipFieldUpdate entryChipFieldUpdate, EntryUpdate entryUpdate)
    {
        switch (entryChipFieldUpdate.Action)
        {
            case MultiEditAction.Add:
                AddCodesToIds(entryUpdate.GlossaryIds, entryChipFieldUpdate.Codes);
                break;
            case MultiEditAction.Set:
                entryUpdate.GlossaryIds = ParseCodesToIds(entryChipFieldUpdate.Codes);
                break;
            case MultiEditAction.Remove:
                entryUpdate.GlossaryIds.Clear();
                break;
            default:
                throw new InvalidEnumArgumentException("Please provide implementation", (int)entryChipFieldUpdate.Action, typeof(MultiEditAction));
        }
        return entryUpdate;
    }

    private async Task<Entry> CreateEntryUpdateRelatedCollection(int id, EntryChipFieldUpdate entryChipFieldUpdate, Entry databaseEntry)
    {
        var entryUpdate = new EntryUpdate
        {
            Id = id,
            RelatedCollectionIds = databaseEntry.RelatedCollections.Select(y => y.Id).ToList()
        };
        entryUpdate = UpdateRelatedCollectionIds(entryChipFieldUpdate, entryUpdate);

        var clientEntry = mapper.Map<Entry>(entryUpdate);
        SetPropertiesFromDbEntry(clientEntry, databaseEntry);
        clientEntry.RelatedCollections = await entryValidationService.ValidateRelatedCollections(entryUpdate);
        return clientEntry;
    }

    private static EntryUpdate UpdateRelatedCollectionIds(EntryChipFieldUpdate entryChipFieldUpdate, EntryUpdate entryUpdate)
    {
        switch (entryChipFieldUpdate.Action)
        {
            case MultiEditAction.Add:
                AddCodesToIds(entryUpdate.RelatedCollectionIds, entryChipFieldUpdate.Codes);
                break;
            case MultiEditAction.Set:
                entryUpdate.RelatedCollectionIds = ParseCodesToIds(entryChipFieldUpdate.Codes);
                break;
            case MultiEditAction.Remove:
                entryUpdate.RelatedCollectionIds.Clear();
                break;
            default:
                throw new InvalidEnumArgumentException("Please provide implementation", (int)entryChipFieldUpdate.Action, typeof(MultiEditAction));
        }
        return entryUpdate;
    }

    private async Task<Entry> CreateEntryHeaderUpdate(EntryHeaderUpdate entryHeaderUpdate, Entry databaseEntry)
    {
        var entryUpdate = new EntryUpdate
        {
            Id = databaseEntry.Id,
            CollectionId = entryHeaderUpdate.CollectionId ?? databaseEntry.CollectionId,
            OfficeId = entryHeaderUpdate.OfficeId ?? databaseEntry.OfficeId,
            Reliability = entryHeaderUpdate.Reliability ?? default,
            Status = entryHeaderUpdate.Status ?? default,
            SubjectIds = entryHeaderUpdate.SubjectIds ?? default,
            DomainIds = entryHeaderUpdate.DomainIds ?? default,
        };
        var clientEntry = mapper.Map<Entry>(entryUpdate);

        if (!entryHeaderUpdate.Status.HasValue)
        {
            clientEntry.Status = databaseEntry.Status;
        }

        if (!entryHeaderUpdate.Reliability.HasValue)
        {
            clientEntry.Reliability = databaseEntry.Reliability;
        }

        clientEntry.Collection = await entryValidationService.ValidateCollection(entryUpdate);
        clientEntry.Office = await entryValidationService.ValidateOffice(entryUpdate);
        clientEntry.Subjects = entryHeaderUpdate.SubjectIds is not null && entryHeaderUpdate.SubjectIds.Any() ? await entryValidationService.ValidateSubjects(entryUpdate) : databaseEntry.Subjects;
        clientEntry.Domains = entryHeaderUpdate.DomainIds is not null && entryHeaderUpdate.DomainIds.Any() ? await entryValidationService.ValidateDomains(entryUpdate) : databaseEntry.Domains;

        return clientEntry;
    }

    private static void AddCodesToIds(ICollection<int> ids, IEnumerable<string> codes)
    {
        var codeIds = ParseCodesToIds(codes);
        foreach (var id in codeIds)
        {
            if (!ids.Contains(id))
            {
                ids.Add(id);
            }
        }
    }

    private static List<int> ParseCodesToIds(IEnumerable<string> codes)
    {
        var ids = new List<int>();
        foreach (var code in codes)
        {
            if (int.TryParse(code, NumberStyles.Any, null, out var id))
            {
                ids.Add(id);
            }
            else
            {
                throw new ArgumentException(@$"Cannot convert code '{code}' to ID.");
            }
        }

        return ids;
    }

    private void SetChangeTrackingFields(Entry clientEntry, Entry databaseEntry)
    {
        clientEntry.Created = databaseEntry.Created;
        clientEntry.CreatedBy = databaseEntry.CreatedBy;
        clientEntry.Deleted = databaseEntry.Deleted;
        clientEntry.Modified = DateTime.UtcNow;
        clientEntry.ModifiedBy = identityService.GetCurrentEmail();
    }

    private Entry CreateEntryUpdateComment(EntryCommentUpdate entryCommentUpdate, Entry databaseEntry)
    {
        var entryUpdate = new EntryUpdate
        {
            Id = databaseEntry.Id,
            CollectionId = databaseEntry.CollectionId,
            OfficeId = databaseEntry.OfficeId,
            Comment = databaseEntry.Comment
        };
        entryUpdate = UpdateComment(entryCommentUpdate, entryUpdate);

        var clientEntry = mapper.Map<Entry>(entryUpdate);

        SetPropertiesFromDbEntry(clientEntry, databaseEntry, false);

        return clientEntry;
    }

    private static EntryUpdate UpdateComment(EntryCommentUpdate entryCommentUpdate, EntryUpdate entryUpdate)
    {
        switch (entryCommentUpdate.Action)
        {
            case MultiEditAction.Add:
                if (string.IsNullOrWhiteSpace(entryUpdate.Comment))
                {
                    entryUpdate.Comment = entryCommentUpdate.Comment;
                }
                else
                {
                    entryUpdate.Comment = entryUpdate.Comment + Environment.NewLine + entryCommentUpdate.Comment;
                }
                break;
            case MultiEditAction.Set:
                entryUpdate.Comment = entryCommentUpdate.Comment;
                break;
            case MultiEditAction.Remove:
                entryUpdate.Comment = null;
                break;
            default:
                throw new InvalidEnumArgumentException("Please provide implementation", (int)entryCommentUpdate.Action, typeof(MultiEditAction));
        }
        return entryUpdate;
    }

    private static string GetFieldValue(EntryLanguage item, string field) => field switch
    {
        EntrySearchAndReplaceUpdate.Definition => item.Definition,
        EntrySearchAndReplaceUpdate.Note => item.Note,
        EntrySearchAndReplaceUpdate.Context => item.Context,
        EntrySearchAndReplaceUpdate.Abbreviation => item.Abbreviation,
        EntrySearchAndReplaceUpdate.Terminus => item.Terminus,
        EntrySearchAndReplaceUpdate.Name => item.Name,
        EntrySearchAndReplaceUpdate.Metadata => item.Metadata,
        EntrySearchAndReplaceUpdate.Phraseology => item.Phraseology,
        EntrySearchAndReplaceUpdate.DefinitionSource => item.DefinitionSource,
        EntrySearchAndReplaceUpdate.NoteSource => item.NoteSource,
        EntrySearchAndReplaceUpdate.ContextSource => item.ContextSource,
        EntrySearchAndReplaceUpdate.AbbreviationSource => item.AbbreviationSource,
        EntrySearchAndReplaceUpdate.TerminusSource => item.TerminusSource,
        EntrySearchAndReplaceUpdate.NameSource => item.NameSource,
        EntrySearchAndReplaceUpdate.MetadataSource => item.MetadataSource,
        EntrySearchAndReplaceUpdate.PhraseologySource => item.PhraseologySource,
        _ => throw new BusinessValidationException(nameof(EntryContentMove.SourceField), $"Field Name {field} is not supported for update")
    };

    private static string GetSourceValueForField(EntryLanguage item, string field) => field switch
    {
        EntrySearchAndReplaceUpdate.Definition => item.DefinitionSource,
        EntrySearchAndReplaceUpdate.Note => item.NoteSource,
        EntrySearchAndReplaceUpdate.Context => item.ContextSource,
        EntrySearchAndReplaceUpdate.Abbreviation => item.AbbreviationSource,
        EntrySearchAndReplaceUpdate.Terminus => item.TerminusSource,
        EntrySearchAndReplaceUpdate.Name => item.NameSource,
        EntrySearchAndReplaceUpdate.Metadata => item.MetadataSource,
        EntrySearchAndReplaceUpdate.Phraseology => item.PhraseologySource,
        _ => throw new BusinessValidationException(nameof(EntryContentMove.SourceField), $"Field Name {field} is not supported for update")
    };

    private static bool TryUpdateSourceField(EntryLanguage item, string sourceField, string value, MultiEditAction action, out bool notFullyUpdated)
    {
        var currentValue = GetFieldValue(item, sourceField);
        var baseValue = GetFieldValue(item, sourceField.Replace("Source", ""));

        // Example for 'DefinitionSource':
        // - The Definition field must be empty.
        // - If adding, the DefinitionSource must also be empty.
        // Note: Failure to meet this rule does NOT prevent modifications to other fields.
        // Same rules apply to other fields like NoteSource, ContextSource, etc.

        var shouldUpdate = !string.IsNullOrEmpty(baseValue) && (action == MultiEditAction.Set || string.IsNullOrWhiteSpace(currentValue));
        notFullyUpdated = string.IsNullOrEmpty(baseValue) || (action == MultiEditAction.Add && !string.IsNullOrWhiteSpace(currentValue));

        if (shouldUpdate)
        {
            SetFieldValue(item, sourceField, value);
            return true;
        }

        return false;
    }

    private static void SetFieldValue(EntryLanguage item, string field, string value)
    {
        switch (field)
        {
            case EntrySearchAndReplaceUpdate.Definition:
                item.Definition = value;
                break;
            case EntrySearchAndReplaceUpdate.Note:
                item.Note = value;
                break;
            case EntrySearchAndReplaceUpdate.Context:
                item.Context = value;
                break;
            case EntrySearchAndReplaceUpdate.Abbreviation:
                item.Abbreviation = value;
                break;
            case EntrySearchAndReplaceUpdate.Terminus:
                item.Terminus = value;
                break;
            case EntrySearchAndReplaceUpdate.Name:
                item.Name = value;
                break;
            case EntrySearchAndReplaceUpdate.Metadata:
                item.Metadata = value;
                break;
            case EntrySearchAndReplaceUpdate.Phraseology:
                item.Phraseology = value;
                break;
            case EntrySearchAndReplaceUpdate.DefinitionSource:
                item.DefinitionSource = value;
                break;
            case EntrySearchAndReplaceUpdate.NoteSource:
                item.NoteSource = value;
                break;
            case EntrySearchAndReplaceUpdate.ContextSource:
                item.ContextSource = value;
                break;
            case EntrySearchAndReplaceUpdate.AbbreviationSource:
                item.AbbreviationSource = value;
                break;
            case EntrySearchAndReplaceUpdate.TerminusSource:
                item.TerminusSource = value;
                break;
            case EntrySearchAndReplaceUpdate.NameSource:
                item.NameSource = value;
                break;
            case EntrySearchAndReplaceUpdate.MetadataSource:
                item.MetadataSource = value;
                break;
            case EntrySearchAndReplaceUpdate.PhraseologySource:
                item.PhraseologySource = value;
                break;
            default:
                throw new BusinessValidationException(nameof(EntryContentMove.SourceField), $"Field {field} is not supported for update");
        }
    }

    private static void SetSourceValueForField(EntryLanguage item, string field, string value)
    {
        switch (field)
        {
            case EntrySearchAndReplaceUpdate.Definition:
                item.DefinitionSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Note:
                item.NoteSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Context:
                item.ContextSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Abbreviation:
                item.AbbreviationSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Terminus:
                item.TerminusSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Name:
                item.NameSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Metadata:
                item.MetadataSource = value;
                break;
            case EntrySearchAndReplaceUpdate.Phraseology:
                item.PhraseologySource = value;
                break;
            default:
                throw new BusinessValidationException(nameof(EntryContentMove.SourceField), $"Field {field} is not supported for update");
        }
    }
}
