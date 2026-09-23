using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Public.Api.Mapping.Actions;

public class EntryLanguageReferencesAction : IMappingAction<EntryLanguage, EntryLanguageDetail>
{
    private readonly ITermdatUrlService termdatUrlService;
    private readonly IBackendTextHelperService backendTextHelperService;

    public EntryLanguageReferencesAction(ITermdatUrlService termdatUrlService, IBackendTextHelperService backendTextHelperService)
    {
        this.termdatUrlService = termdatUrlService;
        this.backendTextHelperService = backendTextHelperService;
    }

    public void Process(EntryLanguage source, EntryLanguageDetail destination, ResolutionContext context)
    {
        if (source.Sequence != 1 || source.Entry?.References?.Any() != true)
        {
            return;
        }

        var references = new List<EntryReferenceSummary>();
        var orderedReferences = source.Entry.References.Where(er => !er.ReferencedEntry.Deleted).OrderBy(er => er.Type).ToList();

        foreach (var reference in orderedReferences)
        {
            var synonym = reference.ReferencedEntry?.Translations
                .Where(t => t.LanguageId == source.LanguageId)
                .OrderBy(t => t.Sequence)
                .FirstOrDefault();

            if (synonym == null)
            {
                continue;
            }

            references.Add(new EntryReferenceSummary
            {
                Text = GetReferenceText(synonym),
                Type = backendTextHelperService.GetReferenceTypeText(reference.Type, source.Language.IsoCode),
                Url = termdatUrlService.GetTermdatUrl(reference.EntryId)
            });
        }

        destination.References = references;
    }

    private static string GetReferenceText(EntryLanguage entryLanguage)
    {
        if (!string.IsNullOrWhiteSpace(entryLanguage.Terminus))
        {
            return entryLanguage.Terminus;
        }

        if (!string.IsNullOrWhiteSpace(entryLanguage.Name))
        {
            return entryLanguage.Name;
        }

        return !string.IsNullOrWhiteSpace(entryLanguage.Phraseology) ? entryLanguage.Phraseology : entryLanguage.Abbreviation;
    }
}
