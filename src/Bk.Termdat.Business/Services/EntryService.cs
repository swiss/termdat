using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Business.Services;

public class EntryService : IEntryService
{
    private readonly IEntryValidationService entryValidationService;
    private readonly IIdentityService identityService;
    private readonly IAuthorizationService authorizationService;
    private readonly IPictureService pictureService;
    private readonly IUserAccountService userAccountService;
    private readonly IAdminInfoService adminInfoService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IEntryRepository entryRepository;
    private readonly ISearchQueueClient searchQueueClient;
    private readonly IMapper mapper;
    private readonly ILogger<EntryService> logger;
    private readonly IEntryNotePatternService entryNotePatternService;

#pragma warning disable S107
    public EntryService(IEntryValidationService entryValidationService,
        IIdentityService identityService,
        IAuthorizationService authorizationService,
        IPictureService pictureService,
        IUserAccountService userAccountService,
        IAdminInfoService adminInfoService,
        IUnitOfWork unitOfWork,
        IEntryRepository entryRepository,
        ISearchQueueClient searchQueueClient,
        IMapper mapper,
        ILogger<EntryService> logger,
        IEntryNotePatternService entryNotePatternService)
#pragma warning restore S107
    {
        this.entryValidationService = entryValidationService;
        this.identityService = identityService;
        this.authorizationService = authorizationService;
        this.pictureService = pictureService;
        this.userAccountService = userAccountService;
        this.adminInfoService = adminInfoService;
        this.unitOfWork = unitOfWork;
        this.entryRepository = entryRepository;
        this.searchQueueClient = searchQueueClient;
        this.mapper = mapper;
        this.logger = logger;
        this.entryNotePatternService = entryNotePatternService;
    }

    public async Task<EntryModificationDetail> Create(EntryCreate entryCreate, string processingUser = null)
    {
        ArgumentNullException.ThrowIfNull(entryCreate);

        entryValidationService.AuthorizeCollection(entryCreate.CollectionId);
        entryValidationService.AuthorizeGlossaries(entryCreate.GlossaryIds);

        // Add new AdminInfos and save them in masterdata first
        await GetOrCreateAdminInfos(entryCreate);

        entryValidationService.ValidateLanguageDetails(entryCreate);
        await entryValidationService.ValidateCollection(entryCreate);
        await entryValidationService.ValidateOffice(entryCreate);
        await entryValidationService.ValidateConceptSystemPosition(entryCreate);

        var mappedEntry = mapper.Map<Entry>(entryCreate);

        mappedEntry.RelatedCollections = await entryValidationService.ValidateRelatedCollections(entryCreate);
        mappedEntry.LenochClassifications = await entryValidationService.ValidateLenochClassification(entryCreate);
        mappedEntry.Subjects = await entryValidationService.ValidateSubjects(entryCreate);
        mappedEntry.Domains = await entryValidationService.ValidateDomains(entryCreate);
        mappedEntry.Glossaries = await entryValidationService.ValidateGlossaries(entryCreate);
        mappedEntry.References = await entryValidationService.ValidateReferences(entryCreate, mappedEntry);
        mappedEntry.AdminInfos = await entryValidationService.ValidateAdminInfos(entryCreate);

        if (string.IsNullOrWhiteSpace(processingUser))
        {
            processingUser = identityService.GetCurrentEmail();
        }

        mappedEntry.ModifiedBy = processingUser;
        mappedEntry.CreatedBy = processingUser;

        var processingDateTime = DateTime.UtcNow;
        mappedEntry.Created = processingDateTime;
        mappedEntry.Modified = processingDateTime;

        await entryRepository.Create(mappedEntry);

        await userAccountService.UpdateLastAction(processingUser);

        await unitOfWork.SaveChangesAsync();

        var documents = await entryRepository.GetByIdForIndex(mappedEntry.Id);
        await searchQueueClient.QueueIndexAdd(documents);

        return await GetModificationDetail(mappedEntry.Id);
    }

    public async Task<EntryModificationDetail> Update(EntryUpdate entryUpdate, string processingUser = null)
    {
        ArgumentNullException.ThrowIfNull(entryUpdate);

        var databaseEntry = await FindEntryForUpdate(entryUpdate.Id);

        if (databaseEntry.Locked.HasValue && databaseEntry.LockedBy != identityService.GetCurrentEmail())
        {
            throw new EntryLockedException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryLocked, entryUpdate.Id));
        }

        entryValidationService.AuthorizeCollection(entryUpdate.CollectionId, databaseEntry.CollectionId);
        entryValidationService.AuthorizeGlossaries(entryUpdate.GlossaryIds, databaseEntry.Glossaries);

        // Add new AdminInfos and save them in masterdata first
        await GetOrCreateAdminInfos(entryUpdate);

        entryValidationService.ValidatePictureFile(entryUpdate.Picture, entryUpdate.Image);
        entryValidationService.ValidateLanguageDetails(entryUpdate);

        var clientEntry = mapper.Map<Entry>(entryUpdate);

        clientEntry.Collection = await entryValidationService.ValidateCollection(entryUpdate);
        clientEntry.Office = await entryValidationService.ValidateOffice(entryUpdate);
        clientEntry.ConceptSystemPosition = await entryValidationService.ValidateConceptSystemPosition(entryUpdate);

        clientEntry.RelatedCollections = await entryValidationService.ValidateRelatedCollections(entryUpdate);
        clientEntry.LenochClassifications = await entryValidationService.ValidateLenochClassification(entryUpdate);
        clientEntry.Glossaries = await entryValidationService.ValidateGlossaries(entryUpdate);
        clientEntry.References = await entryValidationService.ValidateReferences(entryUpdate, clientEntry);
        clientEntry.ExternalLinks = entryValidationService.ValidateExternalLinks(entryUpdate, clientEntry);
        clientEntry.Subjects = await entryValidationService.ValidateSubjects(entryUpdate);
        clientEntry.Domains = await entryValidationService.ValidateDomains(entryUpdate);
        clientEntry.AdminInfos = await entryValidationService.ValidateAdminInfos(entryUpdate);

        SetChangeTrackingFields(clientEntry, databaseEntry, processingUser);

        UpdateLock(clientEntry, databaseEntry);

        // Preserve ExternalSystemId
        clientEntry.ExternalSystemId = databaseEntry.ExternalSystemId;

        var clientPicture = await HandlePicture(entryUpdate, entryUpdate.Image, databaseEntry);
        clientEntry.Picture = await entryValidationService.ValidatePicture(clientPicture);

        if (clientEntry.ConcurrencyToken != databaseEntry.ConcurrencyToken)
        {
            databaseEntry = await entryRepository.GetByIdForEntryDetail(entryUpdate.Id);
            throw new ConcurrencyException(mapper.Map<EntryModificationDetail>(clientEntry), mapper.Map<EntryModificationDetail>(databaseEntry));
        }

        entryRepository.Update(databaseEntry, clientEntry);

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        await unitOfWork.SaveChangesAsync();

        var documents = await entryRepository.GetByIdForIndex(clientEntry.Id);
        await searchQueueClient.QueueIndexUpdate(documents);

        return await GetModificationDetail(databaseEntry.Id);
    }

    private async Task<Picture> HandlePicture(EntryUpdate entryUpdate, IFormFile pictureFile, Entry entry)
    {
        if (entryUpdate.PictureDeleted)
        {
            await pictureService.Delete(entry.Id);
            return null;
        }

        if (entry.Picture != null && entryUpdate.Picture != null)
        {
            entryUpdate.Picture.EntryId = entry.Id;
            return await pictureService.Update(entryUpdate.Picture, pictureFile);
        }

        if (pictureFile != null && entryUpdate.Picture != null)
        {
            entryUpdate.Picture.EntryId = entry.Id;
            return await pictureService.Create(entryUpdate.Picture, pictureFile);
        }

        return null;
    }

    public async Task<EntryDetail> GetDetail(int entryId, IEnumerable<int> sourceLanguageIds = null, IEnumerable<int> targetLanguageIds = null)
    {
        var sourceLanguageIdArray = sourceLanguageIds?.ToArray();
        var targetLanguageIdArray = targetLanguageIds?.ToArray();

        var entry = await entryRepository.GetByIdForEntryDetail(entryId);
        if (entry is null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, entryId));
        }

        // Only admins are allowed to see/know that deleted entries exist
        if (entry.Deleted && !authorizationService.IsAdmin())
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, entryId));
        }

        if (authorizationService.IsWriter())
        {
            try
            {
                entryValidationService.AuthorizeCollection(entry.CollectionId);
                entryValidationService.AuthorizeGlossaries(entry.Glossaries.Select(x => x.Id));
            }
            catch (ForbiddenException)
            {
                // Temporarily remove the comment value if the writer does not have permission to the glossary or collection associated with the entry.
                entry.Comment = null;
            }
        }

        if (authorizationService.IsSuperReader())
        {
            try
            {
                entryValidationService.AuthorizeCollection(entry.CollectionId);
            }
            catch (ForbiddenException)
            {
                // Temporarily remove the comment and adminInfo values if the super-reader does not have permission to the glossary or collection associated with the entry.
                entry.Comment = null;
                entry.AdminInfos = null;
            }
        }

        entry.Translations = entry.Translations
            .Where(t =>
                sourceLanguageIdArray == null || sourceLanguageIdArray.Length == 0 || sourceLanguageIdArray.Contains(t.LanguageId) ||
                targetLanguageIdArray == null || targetLanguageIdArray.Length == 0 || targetLanguageIdArray.Contains(t.LanguageId))
            .ToList();

        entry.Collection.Translations = entry.Collection.Translations.Where(t =>
            sourceLanguageIdArray == null || sourceLanguageIdArray.Length == 0 || sourceLanguageIdArray.Contains(t.LanguageId) ||
            targetLanguageIdArray == null || targetLanguageIdArray.Length == 0 || targetLanguageIdArray.Contains(t.LanguageId)
        ).ToList();

        return mapper.Map<EntryDetail>(entry);
    }

    public async Task<EntryModificationDetail> GetModificationDetail(int entryId)
    {
        var entry = await entryRepository.GetByIdForEntryDetail(entryId);

        if (entry == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, entryId));
        }

        if (!authorizationService.CanEdit(entry))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }

        if (entry.Locked.HasValue && entry.LockedBy != identityService.GetCurrentEmail())
        {
            throw new EntryLockedException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryLocked, entryId));
        }

        return mapper.Map<EntryModificationDetail>(entry);
    }

    public async Task<EntryReferenceDetail> GetReferenceDetail(int entryId)
    {
        var entry = await entryRepository.GetByIdForReferenceDetail(entryId);

        if (entry == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, entryId));
        }

        return mapper.Map<EntryReferenceDetail>(entry);
    }

    public async Task<IEnumerable<EntryMultipleUpdateInfo>> DeleteMultiple(int[] ids, bool soft = true)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var entries = (await entryRepository.GetByIds(ids)).ToList();

        if (soft && entries.Count != ids.Length)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, string.Join(",", ids)));
        }

        if (entries.Exists(e => !authorizationService.CanDelete(e)))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }

        await userAccountService.UpdateLastAction(identityService.GetCurrentEmail());

        if (soft)
        {
            return await SoftDelete(entries);
        }

        var notSoftDeletedInfos = entries
            .Where(item => !item.Deleted)
            .Select(item => new EntryMultipleUpdateInfo { NotSoftDeletedEntryId = item.Id });
        var lockedInfos = await Delete(entries.Where(item => item.Deleted).ToList());

        return notSoftDeletedInfos.Concat(lockedInfos);
    }

    public async Task<IEnumerable<EntryMultipleUpdateInfo>> RestoreMultiple(int[] ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var entries = (await entryRepository.GetByIds(ids)).ToList();

        if (entries.Count != ids.Length)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, string.Join(",", ids)));
        }

        if (entries.Exists(e => !authorizationService.CanRestore(e)))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }

        var lockInfos = entries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy }).ToList();
        var userAccountEmail = identityService.GetCurrentEmail();

        foreach (var entry in entries)
        {
            entry.Deleted = false;
            entry.Modified = DateTime.UtcNow;
            entry.ModifiedBy = userAccountEmail;
        }

        await userAccountService.UpdateLastAction(userAccountEmail);

        await unitOfWork.SaveChangesAsync();

        try
        {
            var documents = await entryRepository.GetByIdsForIndex(ids);
            await searchQueueClient.QueueIndexUpdate(documents);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while updating entries '{Ids}' in search index after restore", string.Join(",", ids));
        }
        return lockInfos;
    }

    public async Task<uint> Lock(int id)
    {
        var entry = await FindEntryForUpdate(id);

        entry.Locked = DateTime.UtcNow;
        entry.LockedBy = identityService.GetCurrentEmail();

        await unitOfWork.SaveChangesAsync();

        return entry.ConcurrencyToken;
    }

    public async Task<uint> Unlock(int id)
    {
        var entry = await FindEntryForUpdate(id);

        entry.Locked = null;
        entry.LockedBy = null;

        await unitOfWork.SaveChangesAsync();

        return entry.ConcurrencyToken;
    }

    public async Task UnlockExpiredEntries()
    {
        var entries = await entryRepository.GetExpiredLockedEntries();

        foreach (var entry in entries)
        {
            entry.Locked = null;
            entry.LockedBy = null;
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<EntryMultipleUpdateResult> MultipleSearchAndReplace(EntrySearchAndReplaceUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var databaseEntries = (await entryRepository.GetByIdsForReplace(update.Ids, update.SearchField, update.SearchValue)).ToList();

        if (databaseEntries.Any(y => !authorizationService.CanEdit(y)))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }

        if (databaseEntries.Exists(y => y.Deleted))
        {
            throw new CanNotBeChangedException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryDeletedCanNotBeEdited, databaseEntries.First(y => y.Deleted).Id));
        }

        var occurrences = 0;
        var affectedEntries = 0;
        var lockInfos = databaseEntries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy }).ToList();

        foreach (var databaseEntry in databaseEntries.Where(x => x.Locked is null))
        {
            var currentOccurrences = PerformSearchAndReplace(databaseEntry, update);
            occurrences += currentOccurrences;

            if (currentOccurrences <= 0)
            {
                continue;
            }

            affectedEntries++;
            databaseEntry.Modified = DateTime.UtcNow;
            databaseEntry.ModifiedBy = identityService.GetCurrentEmail();
        }

        await unitOfWork.SaveChangesAsync();
        await QueueIndexUpdate(update.Ids);

        return new EntryMultipleUpdateResult()
        {
            EntryCount = affectedEntries,
            AffectedOccurrences = occurrences,
            LockInfos = lockInfos
        };
    }

    private async Task<Entry> FindEntryForUpdate(int id)
    {
        var entry = await entryRepository.GetByIdForUpdate(id);

        if (entry is null)
        {
            throw new EntityNotFoundException(BackendTexts.EntryNotFound);
        }

        if (entry.Deleted)
        {
            throw new CanNotBeChangedException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryDeletedCanNotBeEdited, id));
        }

        if (!authorizationService.CanEdit(entry))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }

        return entry;
    }

    private static (string result, int occurrences) PerformReplace(string source, EntrySearchAndReplaceUpdate update)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return (source, 0);
        }

        //whole-words bounded by whitespace or special character  (or at the beginning/end of the source text)
        var replaceRegexPattern = @"(?<=^|\s|,|\(|\[|\{|\-|\.|&|_|=|<|>|#|\)|\]|\}|;|%|/|\\|:)" + Regex.Escape(update.SearchValue) + @"(?=$|\s|,|\(|\[|\{|\-|\.|&|_|=|<|>|#|\)|\]|\}|;|%|/|\\|:)";

        var matches = Regex.Matches(source, replaceRegexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));

        var occurrences = matches.Count;

        return occurrences > 0
            ? (Regex.Replace(source, replaceRegexPattern, update.ReplaceValue ?? string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1)), occurrences)
            : (source, 0);
    }

    private int PerformSearchAndReplace(Entry entry, EntrySearchAndReplaceUpdate update)
    {
        var occurrences = 0;

        if (update.SearchField == EntrySearchAndReplaceUpdate.Comment)
        {
            var replaceResult = PerformReplace(entry.Comment, update);
            entry.Comment = replaceResult.result;
            occurrences = replaceResult.occurrences;
        }
        else
        {
            foreach (var item in entry.Translations.Where(y => update.LanguageIds.Contains(y.LanguageId)))
            {
                if (update.SearchField == EntrySearchAndReplaceUpdate.Definition)
                {
                    var replaceResultDefinition = PerformReplace(item.Definition, update);
                    item.Definition = replaceResultDefinition.result;
                    occurrences += replaceResultDefinition.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.DefinitionSource)
                {
                    var replaceResultDefinitionSource = PerformReplace(item.DefinitionSource, update);
                    item.DefinitionSource = replaceResultDefinitionSource.result;
                    occurrences += replaceResultDefinitionSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.Note)
                {
                    var replaceResultNote = PerformReplace(item.Note, update);
                    item.Note = replaceResultNote.result;
                    occurrences += replaceResultNote.occurrences;
                    (item.IsObsolete, item.IsRare, item.ShouldAvoid) = entryNotePatternService.DetermineEntryNotePatterns(item.Note);
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.NoteSource)
                {
                    var replaceResultNoteSource = PerformReplace(item.NoteSource, update);
                    item.NoteSource = replaceResultNoteSource.result;
                    occurrences += replaceResultNoteSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.Country)
                {
                    var replaceResultCountry = PerformReplace(item.Country, update);
                    item.Country = replaceResultCountry.result;
                    occurrences += replaceResultCountry.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.Context)
                {
                    var replaceResultContext = PerformReplace(item.Context, update);
                    item.Context = replaceResultContext.result;
                    occurrences += replaceResultContext.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.ContextSource)
                {
                    var replaceResultContextSource = PerformReplace(item.ContextSource, update);
                    item.ContextSource = replaceResultContextSource.result;
                    occurrences += replaceResultContextSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.Metadata)
                {
                    var replaceResultMetadata = PerformReplace(item.Metadata, update);
                    item.Metadata = replaceResultMetadata.result;
                    occurrences += replaceResultMetadata.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.MetadataSource)
                {
                    var replaceResultMetadataSource = PerformReplace(item.MetadataSource, update);
                    item.MetadataSource = replaceResultMetadataSource.result;
                    occurrences += replaceResultMetadataSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.TerminusSource)
                {
                    var replaceResultTerminusSource = PerformReplace(item.TerminusSource, update);
                    item.TerminusSource = replaceResultTerminusSource.result;
                    occurrences += replaceResultTerminusSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.NameSource)
                {
                    var replaceResultNameSource = PerformReplace(item.NameSource, update);
                    item.NameSource = replaceResultNameSource.result;
                    occurrences += replaceResultNameSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.AbbreviationSource)
                {
                    var replaceResultAbbreviationSource = PerformReplace(item.AbbreviationSource, update);
                    item.AbbreviationSource = replaceResultAbbreviationSource.result;
                    occurrences += replaceResultAbbreviationSource.occurrences;
                }
                else if (update.SearchField == EntrySearchAndReplaceUpdate.PhraseologySource)
                {
                    var replaceResultPhraseologySource = PerformReplace(item.PhraseologySource, update);
                    item.PhraseologySource = replaceResultPhraseologySource.result;
                    occurrences += replaceResultPhraseologySource.occurrences;
                }
            }
        }
        return occurrences;
    }

    private async Task<IEnumerable<EntryMultipleUpdateInfo>> SoftDelete(List<Entry> entries)
    {
        var lockInfos = entries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy }).ToList();

        foreach (var entry in entries.Where(x => x.Locked is null))
        {
            entry.Deleted = true;
            entry.Modified = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync();
        var ids = entries.Select(e => e.Id);
        try
        {
            var documents = await entryRepository.GetByIdsForIndex(ids);
            await searchQueueClient.QueueIndexUpdate(documents);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while updating entries '{Ids}' in search index after soft delete", string.Join(",", ids));
        }
        return lockInfos;
    }

    private async Task<IEnumerable<EntryMultipleUpdateInfo>> Delete(List<Entry> entries)
    {
        var lockInfos = entries.Where(x => x.Locked != null).Select(e => new EntryMultipleUpdateInfo { LockedEntryId = e.Id, LockedBy = e.LockedBy }).ToList();

        foreach (var entry in entries.Where(x => x.Locked is null))
        {
            entryRepository.Delete(entry);
        }
        await unitOfWork.SaveChangesAsync();

        var ids = entries.Select(e => e.Id);
        try
        {
            await searchQueueClient.QueueIndexDelete(ids);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while deleting entries '{Ids}' in index after delete", string.Join(",", ids));
        }
        return lockInfos;
    }

    private void SetChangeTrackingFields(Entry clientEntry, Entry databaseEntry, string processingUser)
    {
        if (string.IsNullOrWhiteSpace(processingUser))
        {
            processingUser = identityService.GetCurrentEmail();
        }

        clientEntry.Created = databaseEntry.Created;
        clientEntry.CreatedBy = databaseEntry.CreatedBy;
        clientEntry.Deleted = databaseEntry.Deleted;
        clientEntry.Modified = DateTime.UtcNow;
        clientEntry.ModifiedBy = processingUser;
    }

    private static void UpdateLock(Entry clientEntry, Entry databaseEntry)
    {
        clientEntry.Locked = databaseEntry.Locked != null ? DateTime.UtcNow : null;
        clientEntry.LockedBy = databaseEntry.LockedBy;
    }

    private async Task QueueIndexUpdate(IEnumerable<int> updateIds)
    {
        var entriesToUpdate = await entryRepository.GetByIdsForIndex(updateIds);
        await searchQueueClient.QueueIndexUpdate(entriesToUpdate);
    }

    private async Task GetOrCreateAdminInfos(IEntryModification entry)
    {
        if (entry.AdminInfos != null)
        {
            foreach (var newAdminInfo in entry.AdminInfos.Where(a => a.Id == null))
            {
                var adminInfoDetail = await adminInfoService.GetOrCreateAdminInfos(newAdminInfo);
                newAdminInfo.Id = adminInfoDetail.Id;
            }
        }
    }
}
