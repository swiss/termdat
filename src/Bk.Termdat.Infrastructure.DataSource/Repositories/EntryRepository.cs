using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class EntryRepository : IEntryRepository
{
    private readonly DataContext context;

    public EntryRepository(DataContext context)
    {
        this.context = context;
    }

    public Task<int> Count()
    {
        return context.Entries.CountAsync();
    }

    public async Task<Entry> GetById(int id)
    {
        return await context.Entries.FindAsync(id);
    }

    public Task<Entry> GetByIdForEntryDetail(int id)
    {
        return context.Entries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.LenochClassifications)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Subjects)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Domains)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Office)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.RelatedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Glossaries)
                .ThenInclude(g => g.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.ConceptSystemPosition)
            .Include(e => e.Picture)
                .ThenInclude(p => p.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.References)
                .ThenInclude(r => r.ReferencedEntry)
                    .ThenInclude(re => re.Translations)
                        .ThenInclude(t => t.Language)
            .Include(e => e.ExternalLinks)
            .Include(e => e.ReferencedByEntries)
                .ThenInclude(r => r.Entry)
                    .ThenInclude(re => re.Translations)
                        .ThenInclude(t => t.Language)
            .Include(e => e.AdminInfos)
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Entry>> GetByIdsForExport(IEnumerable<int> ids)
    {
        var entries = await context.Entries
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.LenochClassifications)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Subjects)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Domains)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Office)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.RelatedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Glossaries)
                .ThenInclude(g => g.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.ConceptSystemPosition)
            .Include(e => e.Picture)
                .ThenInclude(p => p.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Picture)
                .ThenInclude(p => p.File)
            .Include(e => e.References)
                .ThenInclude(r => r.ReferencedEntry)
                    .ThenInclude(re => re.Translations)
                        .ThenInclude(t => t.Language)
            .Include(e => e.ReferencedByEntries)
                .ThenInclude(r => r.Entry)
                    .ThenInclude(re => re.Translations)
                        .ThenInclude(t => t.Language)
            .Include(e => e.AdminInfos)
            .Include(e => e.ExternalLinks)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries.OrderBy(e => ids.ToList().IndexOf(e.Id));
    }

    public Task<Entry> GetByIdForUpdate(int id)
    {
        return context.Entries
            .Include(e => e.Translations)
            .Include(e => e.Collection)
            .Include(e => e.LenochClassifications)
            .Include(e => e.Office)
            .Include(e => e.RelatedCollections)
            .Include(e => e.Glossaries)
            .Include(e => e.ConceptSystemPosition)
            .Include(e => e.Picture)
                .ThenInclude(p => p.File)
            .Include(e => e.Picture)
                .ThenInclude(p => p.Translations)
            .Include(e => e.References)
            .Include(e => e.ExternalLinks)
            .Include(e => e.Subjects)
            .Include(e => e.Domains)
            .Include(e => e.AdminInfos)
            .AsSplitQuery()
            .SingleOrDefaultAsync(e => e.Id == id);
    }

    public Task<Entry> GetByExternalSystemId(string externalSystemId)
    {
        return context.Entries
            .Include(e => e.Translations)
            .Include(e => e.Collection)
            .Include(e => e.LenochClassifications)
            .Include(e => e.Office)
            .Include(e => e.RelatedCollections)
            .Include(e => e.Glossaries)
            .Include(e => e.ConceptSystemPosition)
            .Include(e => e.Picture)
            .ThenInclude(p => p.File)
            .Include(e => e.Picture)
            .ThenInclude(p => p.Translations)
            .Include(e => e.References)
            .Include(e => e.ExternalLinks)
            .Include(e => e.Subjects)
            .Include(e => e.Domains)
            .Include(e => e.AdminInfos)
            .AsSplitQuery()
            .SingleOrDefaultAsync(e => e.ExternalSystemId == externalSystemId);
    }

    public async Task<IEnumerable<Entry>> GetByIdsForUpdate(IEnumerable<int> ids)
    {
        return await context.Entries
            .Include(e => e.Translations)
            .Include(e => e.Collection)
            .Include(e => e.LenochClassifications)
            .Include(e => e.Office)
            .Include(e => e.RelatedCollections)
            .Include(e => e.Glossaries)
            .Include(e => e.ConceptSystemPosition)
            .Include(e => e.Picture)
                .ThenInclude(p => p.File)
            .Include(e => e.Picture)
                .ThenInclude(p => p.Translations)
            .Include(e => e.References)
            .Include(e => e.ExternalLinks)
            .Include(e => e.Subjects)
            .Include(e => e.Domains)
            .Include(e => e.AdminInfos)
            .Include(e => e.Subjects)
            .Include(e => e.Domains)
            .AsSplitQuery()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entry>> GetByIdsForReplace(IEnumerable<int> ids, string searchField, string searchText)
    {
        return await context.Entries
            .Include(e => e.Translations)
            .AsSplitQuery()
            .Where(e => ids.Contains(e.Id) && ((searchField == EntrySearchAndReplaceUpdate.Comment && e.Comment.Contains(searchText)) ||
            (searchField == EntrySearchAndReplaceUpdate.Country && e.Translations.Any(t => t.Country.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.Definition && e.Translations.Any(t => t.Definition.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.Note && e.Translations.Any(t => t.Note.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.Context && e.Translations.Any(t => t.Context.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.Metadata && e.Translations.Any(t => t.Metadata.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.DefinitionSource && e.Translations.Any(t => t.DefinitionSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.NoteSource && e.Translations.Any(t => t.NoteSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.ContextSource && e.Translations.Any(t => t.ContextSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.MetadataSource && e.Translations.Any(t => t.MetadataSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.TerminusSource && e.Translations.Any(t => t.TerminusSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.NameSource && e.Translations.Any(t => t.NameSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.AbbreviationSource && e.Translations.Any(t => t.AbbreviationSource.Contains(searchText))) ||
            (searchField == EntrySearchAndReplaceUpdate.PhraseologySource && e.Translations.Any(t => t.PhraseologySource.Contains(searchText)))))
            .ToListAsync();
    }

    public async Task<IEnumerable<Entry>> GetExpiredLockedEntries(int expirationMinutes = 120)
    {
        return await context.Entries
            .Where(e => e.Locked.HasValue && e.Locked.Value.AddMinutes(expirationMinutes) <= DateTime.UtcNow)
            .ToListAsync();
    }

    public Task<Entry> GetByIdForReferenceDetail(int id)
    {
        return context.Entries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .AsSplitQuery()
            .SingleOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Entry>> GetByIds(IEnumerable<int> ids)
    {
        return await context.Entries.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public IAsyncEnumerable<IndexEntry> GetPageForIndex(int skip, int take)
    {
        return GetForIndexQuery()
            .Skip(skip)
            .Take(take)
            .SelectMany(e => e.Translations, (entry, translation) => CreateIndexEntry(entry, translation))
            .AsAsyncEnumerable();
    }

    public async Task<IEnumerable<Entry>> GetByIdsForSearchResultsAdminOrWriterOrSuperReader(IEnumerable<int> ids)
    {
        var entryIdList = ids.ToList();

        var entries = await context.Entries
            .Where(e => entryIdList.Contains(e.Id))
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Glossaries)
            .Include(e => e.Translations)
                .ThenInclude(l => l.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries
            .OrderBy(e => entryIdList.IndexOf(e.Id));
    }

    public async Task<IEnumerable<Entry>> GetByIdsForSearchResults(IEnumerable<int> ids)
    {
        var entryIdList = ids.ToList();

        var entries = await context.Entries
            .Where(e => entryIdList.Contains(e.Id))
            .Where(e => !e.Deleted)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.Glossaries)
            .Include(e => e.Translations)
                .ThenInclude(l => l.Language)
            .Where(e => e.Collection.Active)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries
            .OrderBy(e => entryIdList.IndexOf(e.Id));
    }

    public async Task<IEnumerable<Entry>> GetByIdsForApiDetail(IEnumerable<int> ids)
    {
        var entryIdList = ids.ToList();

        var entries = await context.Entries
            .Where(e => entryIdList.Contains(e.Id))
            .Include(e => e.Office)
                .ThenInclude(o => o.Translations)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Classification)
                    .ThenInclude(c => c.Translations)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(e => e.References)
                .ThenInclude(r => r.ReferencedEntry)
                    .ThenInclude(re => re.Translations)
            .Include(e => e.ExternalLinks)
            .Include(e => e.Subjects)
                .ThenInclude(s => s.Translations)
            .Include(e => e.Domains)
                .ThenInclude(d => d.Translations)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries
            .OrderBy(e => entryIdList.IndexOf(e.Id));
    }

    public async Task<IEnumerable<Entry>> GetByIdsForApiSummary(IEnumerable<int> ids)
    {
        var entryIdList = ids.ToList();

        var entries = await context.Entries
            .Where(e => entryIdList.Contains(e.Id))
            .Include(e => e.Office)
                .ThenInclude(o => o.Translations)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Classification)
                    .ThenInclude(c => c.Translations)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
            .Include(e => e.Translations)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries
            .OrderBy(e => entryIdList.IndexOf(e.Id));
    }


    public async Task<IEnumerable<Entry>> GetByIdsForWatchlist(IEnumerable<int> ids)
    {
        var entryIds = ids.ToList();

        var entries = await context.Entries
            .Include(e => e.AdminInfos)
            .Include(e => e.Glossaries)
            .Include(e => e.Translations)
                .ThenInclude(l => l.Language)
            .Include(e => e.Collection)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(e => e.ConceptSystemPosition)
            .Where(e => entryIds.Contains(e.Id))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entries
            .OrderBy(e => entryIds.IndexOf(e.Id));
    }

    public async Task<IEnumerable<IndexEntry>> GetByIdForIndex(int entryId)
    {
        return await GetForIndexQuery()
            .Where(e => e.Id == entryId)
            .SelectMany(e => e.Translations, (entry, translation) => CreateIndexEntry(entry, translation))
            .ToListAsync();
    }

    public async Task<IEnumerable<IndexEntry>> GetByIdsForIndex(IEnumerable<int> ids)
    {
        return await GetForIndexQuery()
            .Where(e => ids.Contains(e.Id))
            .SelectMany(e => e.Translations, (entry, translation) => CreateIndexEntry(entry, translation))
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetIdsByCollectionId(int collectionId)
    {
        return await context.Entries
            .Where(e => e.CollectionId == collectionId)
            .Select(e => e.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetIdsByGlossaryId(int glossaryId)
    {
        return await context.Entries
            .Where(e => e.Glossaries.Any(g => g.Id == glossaryId))
            .Select(e => e.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetIdsByConceptPositionIds(IEnumerable<int> positionIds)
    {
        return await context.Entries
            .Where(e => e.ConceptSystemPosition != null && positionIds.Contains(e.ConceptSystemPositionId.Value))
            .Select(e => e.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetIdsByAdminInfoId(int adminInfoId)
    {
        return await context.Entries
            .Where(e => e.AdminInfos.Any(g => g.Id == adminInfoId))
            .Select(e => e.Id)
            .ToListAsync();
    }

    public void Delete(Entry entry)
    {
        context.Remove(entry);
    }

    public async Task Create(Entry entry)
    {
        await context.Entries.AddAsync(entry);
    }

    public void Update(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslations(existing, update);
        UpdateManyToManyRelation(existing.RelatedCollections, update.RelatedCollections);
        UpdateManyToManyRelation(existing.LenochClassifications, update.LenochClassifications);
        UpdateManyToManyRelation(existing.Glossaries, update.Glossaries);
        UpdateManyToManyRelation(existing.Subjects, update.Subjects);
        UpdateManyToManyRelation(existing.Domains, update.Domains);
        UpdateManyToManyRelation(existing.AdminInfos, update.AdminInfos);
        UpdateReferences(existing.References, update.References);
        UpdateExternalLinks(existing.ExternalLinks, update.ExternalLinks);
    }

    public void UpdateGlossaries(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        UpdateManyToManyRelation(existing.Glossaries, update.Glossaries);
    }

    public void UpdateRelatedCollections(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        UpdateManyToManyRelation(existing.RelatedCollections, update.RelatedCollections);
    }

    public void UpdateAdminInfos(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        UpdateManyToManyRelation(existing.AdminInfos, update.AdminInfos);
    }

    public void UpdateWithoutRelations(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
    }

    public void UpdateMandatoryHeaderFields(Entry existing, Entry update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        existing.Subjects = update.Subjects;
        existing.Domains = update.Domains;
    }

    private void UpdateReferences(ICollection<EntryReference> existingReferences, ICollection<EntryReference> updateReferences)
    {
        if (!existingReferences.Any() && !updateReferences.Any())
        {
            return;
        }

        var existingEntryReferenceIds = existingReferences.Select(c => c.Id).ToArray();
        var updatedEntryReferenceIds = updateReferences.Where(er => er.Id > default(int)).Select(c => c.Id).ToArray();
        var removedEntryReferenceIds = existingEntryReferenceIds.Except(updatedEntryReferenceIds);
        foreach (var removedReferenceId in removedEntryReferenceIds)
        {
            var removeEntity = existingReferences.SingleOrDefault(c => c.Id == removedReferenceId);
            if (removeEntity != null)
            {
                context.EntryReferences.Remove(removeEntity);
            }
        }

        foreach (var addEntity in updateReferences.Where(er => er.Id <= default(int)))
        {
            context.EntryReferences.Add(addEntity);
        }

        foreach (var updatedReferenceId in updatedEntryReferenceIds)
        {
            var updateEntity = updateReferences.Single(c => c.Id == updatedReferenceId);
            var existingEntity = existingReferences.Single(c => c.Id == updatedReferenceId);
            context.Entry(existingEntity).CurrentValues.SetValues(updateEntity);
        }
    }

    private void UpdateExternalLinks(
        ICollection<EntryExternalLink> existingExternalLinks,
        ICollection<EntryExternalLink> updateExternalLinks)
    {
        if (!existingExternalLinks.Any() && !updateExternalLinks.Any())
        {
            return;
        }

        var existingEntryExternalLinkIds = existingExternalLinks.Select(c => c.Id).ToArray();
        var updatedEntryExternalLinkIds = updateExternalLinks.Where(er => er.Id > default(int)).Select(c => c.Id).ToArray();
        var removedEntryExternalLinkIds = existingEntryExternalLinkIds.Except(updatedEntryExternalLinkIds);
        foreach (var removedExternalLinkId in removedEntryExternalLinkIds)
        {
            var removeEntity = existingExternalLinks.SingleOrDefault(c => c.Id == removedExternalLinkId);
            if (removeEntity != null)
            {
                context.EntryExternalLinks.Remove(removeEntity);
            }
        }

        foreach (var addEntity in updateExternalLinks.Where(er => er.Id <= default(int)))
        {
            context.EntryExternalLinks.Add(addEntity);
        }

        foreach (var updatedExternalLinkId in updatedEntryExternalLinkIds)
        {
            var updateEntity = updateExternalLinks.Single(c => c.Id == updatedExternalLinkId);
            var existingEntity = existingExternalLinks.Single(c => c.Id == updatedExternalLinkId);
            context.Entry(existingEntity).CurrentValues.SetValues(updateEntity);
        }
    }

    private void UpdateTranslations(Entry existing, Entry update)
    {
        foreach (var updateTranslation in update.Translations)
        {
            updateTranslation.EntryId = update.Id;

            var existingTranslation = updateTranslation.Id > 0 ? existing.Translations.FirstOrDefault(p => p.Id == updateTranslation.Id) : null;

            if (existingTranslation == null)
            {
                existing.Translations.Add(updateTranslation);
            }
            else
            {
                context.Entry(existingTranslation).CurrentValues.SetValues(updateTranslation);
            }
        }

        foreach (var existingTranslation in existing.Translations)
        {
            if (update.Translations.All(t => t.Id != existingTranslation.Id))
            {
                context.Remove(existingTranslation);
            }
        }
    }

    private static void UpdateManyToManyRelation<T>(ICollection<T> existingEntities, ICollection<T> updatedEntities)
        where T : Entity
    {
        var existingEntityIds = existingEntities.Select(c => c.Id).ToArray();
        var updatedEntityIds = updatedEntities.Select(c => c.Id).ToArray();
        var addEntityIds = updatedEntityIds.Except(existingEntityIds);
        var removeEntityIds = existingEntityIds.Except(updatedEntityIds);
        foreach (var removeEntityId in removeEntityIds)
        {
            var removeEntity = existingEntities.Single(c => c.Id == removeEntityId);
            existingEntities.Remove(removeEntity);
        }

        foreach (var addEntityId in addEntityIds)
        {
            var addEntity = updatedEntities.Single(c => c.Id == addEntityId);
            existingEntities.Add(addEntity);
        }
    }

    private static IndexEntry CreateIndexEntry(Entry entry, EntryLanguage translation)
    {
        return new IndexEntry
        {
            EntryId = entry.Id,
            OfficeId = entry.OfficeId,
            CollectionId = entry.CollectionId,
            CollectionIsActive = entry.Collection.Active,
            RelatedCollections = entry.RelatedCollections?.Select(g => g.Id).ToArray() ?? Array.Empty<int>(),
            LenochClassifications = entry.LenochClassifications?.Select(d => d.Id).ToArray() ?? Array.Empty<int>(),
            Glossaries = entry.Glossaries?.Select(g => g.Id).ToArray() ?? Array.Empty<int>(),
            References = entry.References?.Select(r => r.ReferencedEntryId).ToArray() ?? Array.Empty<int>(),
            Subjects = entry.Subjects?.Select(s => s.Id).ToArray() ?? Array.Empty<int>(),
            Domains = entry.Domains?.Select(d => d.Id).ToArray() ?? Array.Empty<int>(),
            AdminInfos = entry.AdminInfos?.Select(a => a.Id).ToArray() ?? Array.Empty<int>(),
            Comment = entry.Comment,
            Status = entry.Status,
            Reliability = entry.Reliability,
            LanguageId = translation.LanguageId,
            SequenceId = translation.Sequence,
            Terminus = translation.Terminus,
            TerminusSource = translation.TerminusSource,
            Name = translation.Name,
            NameSource = translation.NameSource,
            Abbreviation = translation.Abbreviation,
            AbbreviationSource = translation.AbbreviationSource,
            Phraseology = translation.Phraseology,
            PhraseologySource = translation.PhraseologySource,
            Definition = translation.Definition,
            DefinitionSource = translation.DefinitionSource,
            Note = translation.Note,
            NoteSource = translation.NoteSource,
            Context = translation.Context,
            ContextSource = translation.ContextSource,
            Country = translation.Country,
            Metadata = translation.Metadata,
            MetadataSource = translation.MetadataSource,
            CreatedBy = entry.CreatedBy,
            Modified = entry.Modified,
            ModifiedBy = entry.ModifiedBy,
            Deleted = entry.Deleted,
            LanguageCount = entry.Translations.GroupBy(x => x.LanguageId).Count()
        };
    }

    private IQueryable<Entry> GetForIndexQuery()
    {
        return context.Entries
            .Include(e => e.Collection)
            .Include(e => e.Translations)
            .Include(e => e.LenochClassifications)
            .Include(e => e.Glossaries)
            .Include(e => e.RelatedCollections)
            .Include(e => e.References)
            .Include(e => e.Subjects)
            .Include(e => e.Domains)
            .Include(e => e.AdminInfos)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(e => e.Id);
    }
}
