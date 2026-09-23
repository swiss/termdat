using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;

namespace Bk.Termdat.Business.Mapping.Actions;

public class ExportWatchlistEntryLanguageItemAction : IMappingAction<Entry, WatchlistExportEntry>
{
    private readonly ITermdatUrlService termdatUrlService;

    public ExportWatchlistEntryLanguageItemAction(ITermdatUrlService termdatUrlService)
    {
        this.termdatUrlService = termdatUrlService;
    }

    public void Process(Entry source, WatchlistExportEntry destination, ResolutionContext context)
    {
        destination.LanguageItems = source.Translations
            .OrderBy(item => item.Language.SortIndex)
            .ThenBy(item => item.Sequence)
            .GroupBy(item => item.Language.IsoCode)
            .Select(group =>
            {
                return new WatchlistExportEntryLanguageItem
                {
                    IsoCode = group.Key,
                    Details = group.Select(detail => new WatchlistExportEntryLanguageDetail
                    {
                        TerminusKey = BackendTexts.EntryLanguageTerminus,
                        Terminus = detail.Terminus,
                        TerminusSource = ToHtmlText(detail.TerminusSource),
                        NameKey = BackendTexts.EntryLanguageName,
                        Name = detail.Name,
                        NameSource = ToHtmlText(detail.NameSource),
                        AbbreviationKey = BackendTexts.EntryLanguageAbbreviation,
                        Abbreviation = detail.Abbreviation,
                        AbbreviationSource = ToHtmlText(detail.AbbreviationSource),
                        PhraseologyKey = BackendTexts.EntryLanguagePhraseology,
                        Phraseology = detail.Phraseology,
                        PhraseologySource = ToHtmlText(detail.PhraseologySource),
                        DefinitionKey = BackendTexts.EntryLanguageDefinition,
                        Definition = detail.Definition,
                        DefinitionSource = ToHtmlText(detail.DefinitionSource),
                        NoteKey = BackendTexts.EntryLanguageNote,
                        Note = detail.Note,
                        NoteSource = ToHtmlText(detail.NoteSource),
                        ContextKey = BackendTexts.EntryLanguageContext,
                        Context = detail.Context,
                        ContextSource = ToHtmlText(detail.ContextSource),
                        CountryKey = BackendTexts.EntryLanguageCountry,
                        Country = detail.Country,
                        MetadataKey = BackendTexts.EntryLanguageMetadata,
                        Metadata = detail.Metadata,
                        ReferencesKey = BackendTexts.EntryLanguageReference,
                        References = detail.Sequence == 1 ? GetReferenceTexts(source.References, group.Key) : string.Empty
                    }).ToList(),
                };
            }).ToList();
    }

    private string GetReferenceTexts(ICollection<EntryReference> references, string language)
    {
        var sb = new StringBuilder();

        // zuerst solche ohne Typ
        var noType = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.None);

        if (!string.IsNullOrWhiteSpace(noType))
        {
            sb.Append(noType + "; ");
        }

        // dann GEN
        var genText = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.General);

        if (!string.IsNullOrWhiteSpace(genText))
        {
            sb.Append(genText + "; ");
        }

        // dann SPE
        var speText = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.Specific);

        if (!string.IsNullOrWhiteSpace(speText))
        {
            sb.Append(speText + "; ");
        }

        // dann ANT
        var antText = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.Antonym);

        if (!string.IsNullOrWhiteSpace(antText))
        {
            sb.Append(antText + "; ");
        }

        // dann TOUT
        var toutText = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.Whole);

        if (!string.IsNullOrWhiteSpace(toutText))
        {
            sb.Append(toutText + "; ");
        }

        // dann PART
        var partText = GetReferenceTextByTypeAndLanguage(references, language, ReferenceType.Part);

        if (!string.IsNullOrWhiteSpace(partText))
        {
            sb.Append(partText + "; ");
        }

        if (sb.Length > 0)
        {
            return sb.ToString().Substring(0, sb.Length - 2);
        }

        return string.Empty;
    }

    private string GetReferenceTextByTypeAndLanguage(ICollection<EntryReference> references, string language, ReferenceType type)
    {
        var hasData = false;

        var prefix = type switch
        {
            ReferenceType.General => BackendTexts.ReferenceTypeGeneralCode,
            ReferenceType.Specific => BackendTexts.ReferenceTypeSpecificCode,
            ReferenceType.Antonym => BackendTexts.ReferenceTypeAntonymCode,
            ReferenceType.Whole => BackendTexts.ReferenceTypeWholeCode,
            ReferenceType.Part => BackendTexts.ReferenceTypePartCode,
            ReferenceType.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(prefix))
        {
            sb.Append(prefix + ": ");
        }

        var entryLanguages = references
            .Where(entry => !entry.ReferencedEntry.Deleted)
            .Where(entry => entry.Type == type)
            .Select(referencedEntry => referencedEntry.ReferencedEntry.Translations
                .Where(entryLanguage => entryLanguage.Language.IsoCode == language)
                .OrderBy(entryLanguage => entryLanguage.Sequence)
                .FirstOrDefault()
        );

        foreach (var entry in entryLanguages)
        {
            hasData = true;
            sb.Append(GetReferenceHtmlText(entry));
        }

        if (hasData && sb.Length > 0)
        {
            return sb.ToString().Substring(0, sb.Length - 2);
        }

        return string.Empty;
    }

    private string GetReferenceHtmlText(EntryLanguage entryLanguage)
    {
        if (entryLanguage is null)
        {
            return null;
        }

        var value = string.Empty;
        if (!string.IsNullOrWhiteSpace(entryLanguage.Terminus))
        {
            value += entryLanguage.Terminus;
        }
        else if (!string.IsNullOrWhiteSpace(entryLanguage.Name))
        {
            value += entryLanguage.Name;
        }
        else
        {
            value += !string.IsNullOrWhiteSpace(entryLanguage.Phraseology)
                ? entryLanguage.Phraseology
                : entryLanguage.Abbreviation;
        }

        if (value.Length > 0)
        {
            // Make a html-link out of reference pointing to referenced entry
            value = $"<a href=\"{termdatUrlService.GetTermdatUrl(entryLanguage.EntryId)}\">{value}</a>, ";
        }

        return value;
    }

    /// <summary>
    /// Find and replace all Markdown-Links [...](...) occurrences with respective html tags <a href="...">...</a>
    /// </summary>
    private static string ToHtmlText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var regex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)");
        return regex.Replace(text, "<a href=\"$2\">$1</a>");
    }
}
