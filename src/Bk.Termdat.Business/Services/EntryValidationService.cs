using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.Common.Utilities;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services;

public class EntryValidationService : IEntryValidationService
{
    private readonly IAuthorizationService authorizationService;
    private readonly ICollectionRepository collectionRepository;
    private readonly IOfficeRepository officeRepository;
    private readonly IConceptSystemPositionRepository conceptSystemPositionRepository;
    private readonly ILenochClassificationRepository lenochClassificationRepository;
    private readonly ILanguageRepository languageRepository;
    private readonly ISubjectRepository subjectRepository;
    private readonly IDomainRepository domainRepository;
    private readonly IAdminInfoRepository adminInfoRepository;
    private readonly IEntryRepository entryRepository;
    private readonly IEntryNotePatternService entryNotePatternService;
    private readonly IGlossaryRepository glossaryRepository;
    private const int MaxSizeByte = 2097152;

#pragma warning disable S107
    public EntryValidationService(
        IAuthorizationService authorizationService,
        ICollectionRepository collectionRepository,
        IOfficeRepository officeRepository,
        IConceptSystemPositionRepository conceptSystemPositionRepository,
        ILenochClassificationRepository lenochClassificationRepository,
        ILanguageRepository languageRepository,
        IAdminInfoRepository adminInfoRepository,
        IEntryRepository entryRepository,
        IEntryNotePatternService entryNotePatternService,
        IGlossaryRepository glossaryRepository,
        ISubjectRepository subjectRepository,
        IDomainRepository domainRepository
#pragma warning restore S107
    )
    {
        this.authorizationService = authorizationService;
        this.collectionRepository = collectionRepository;
        this.officeRepository = officeRepository;
        this.conceptSystemPositionRepository = conceptSystemPositionRepository;
        this.lenochClassificationRepository = lenochClassificationRepository;
        this.languageRepository = languageRepository;
        this.adminInfoRepository = adminInfoRepository;
        this.subjectRepository = subjectRepository;
        this.entryRepository = entryRepository;
        this.entryNotePatternService = entryNotePatternService;
        this.glossaryRepository = glossaryRepository;
        this.subjectRepository = subjectRepository;
        this.domainRepository = domainRepository;
    }

    public void ValidateLanguageDetails(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        var languageDetailsByLanguage = entryModification.LanguageDetails.GroupBy(ld => ld.LanguageId);
        foreach (var languageDetails in languageDetailsByLanguage)
        {
            const int startSequence = 1;

            var expectedSequences = Enumerable.Range(startSequence, languageDetails.Count()).ToArray();
            var actualSequences = languageDetails.Select(ld => ld.Sequence).ToArray();

            if (!expectedSequences.SequenceEqual(actualSequences))
            {
                throw new BusinessValidationException(
                    nameof(EntryCreate.LanguageDetails),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryLanguageDetailsSequenceInvalid, string.Join(",", expectedSequences), string.Join(",", actualSequences), languageDetails.Key));
            }

            foreach (var detail in languageDetails)
            {
                (detail.IsObsolete, detail.IsRare, detail.ShouldAvoid) = entryNotePatternService.DetermineEntryNotePatterns(detail.Note);
                EscapeBracketsInHyperlinksInSources(detail);
            }
        }
    }

    public void EscapeBracketsInHyperlinksInSources(EntryModificationLanguageDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);

        detail.AbbreviationSource = EncodeNonEmptyHyperlinks(detail.AbbreviationSource);
        detail.ContextSource = EncodeNonEmptyHyperlinks(detail.ContextSource);
        detail.DefinitionSource = EncodeNonEmptyHyperlinks(detail.DefinitionSource);
        detail.MetadataSource = EncodeNonEmptyHyperlinks(detail.MetadataSource);
        detail.NameSource = EncodeNonEmptyHyperlinks(detail.NameSource);
        detail.NoteSource = EncodeNonEmptyHyperlinks(detail.NoteSource);
        detail.PhraseologySource = EncodeNonEmptyHyperlinks(detail.PhraseologySource);
        detail.TerminusSource = EncodeNonEmptyHyperlinks(detail.TerminusSource);
    }

    public async Task<Collection> ValidateCollection(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        var requestedCollection = await collectionRepository.GetById(entryModification.CollectionId);
        if (requestedCollection == null)
        {
            throw new BusinessValidationException(
                nameof(EntryCreate.CollectionId),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.CollectionNotFound, entryModification.CollectionId));
        }

        return requestedCollection;
    }

    public void AuthorizeCollection(int collectionId, int? existingCollectionId = null)
    {
        if (authorizationService.IsAdmin() || authorizationService.IsSuperUser())
        {
            return;
        }

        var availableCollections = authorizationService.GetAuthorizedCollectionIds().ToList();

        if (existingCollectionId.HasValue)
        {
            availableCollections.Add(existingCollectionId.Value);
        }

        if (!availableCollections.Contains(collectionId))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }
    }

    public async Task<Office> ValidateOffice(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        var requestedOffice = await officeRepository.GetById(entryModification.OfficeId);
        if (requestedOffice == null)
        {
            throw new BusinessValidationException(
                nameof(EntryCreate.OfficeId),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryOfficeNotFound, entryModification.OfficeId));
        }

        return requestedOffice;
    }

    public async Task<ConceptSystemPosition> ValidateConceptSystemPosition(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (!entryModification.ConceptSystemPositionId.HasValue)
        {
            return null;
        }

        var conceptSystemPosition = await conceptSystemPositionRepository.GetById(entryModification.ConceptSystemPositionId.Value);

        if (conceptSystemPosition == null)
        {
            throw new BusinessValidationException(
                nameof(EntryCreate.ConceptSystemPositionId),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.ConceptSystemNotFound, entryModification.ConceptSystemPositionId.Value));
        }

        return conceptSystemPosition;

    }

    public void ValidatePictureFile(PictureDetail picture, IFormFile pictureFile)
    {
        if (pictureFile != null)
        {
            if (picture == null)
            {
                throw new BusinessValidationException(
                    nameof(picture),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.PictureMinimalFields));
            }

            if (pictureFile.Length > MaxSizeByte)
            {
                throw new BusinessValidationException(
                    nameof(pictureFile),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.PictureFileSize, ((long)MaxSizeByte).ToSize(SizeExtension.SizeUnits.KB)));
            }
        }
    }

    public async Task<IList<LenochClassification>> ValidateLenochClassification(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.LenochClassificationIds == null || !entryModification.LenochClassificationIds.Any())
        {
            return new List<LenochClassification>();
        }

        var requestedLenochClassifications = (await lenochClassificationRepository.GetByIds(entryModification.LenochClassificationIds))
            .ToList();

        var requestedLenochClassificationIds = entryModification.LenochClassificationIds.OrderBy(id => id).ToArray();
        var loadedLenochClassificationIds = requestedLenochClassifications.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedLenochClassificationIds.SequenceEqual(loadedLenochClassificationIds))
        {
            throw new BusinessValidationException(
                nameof(entryModification.LenochClassificationIds),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryLenochClassificationsNotFound, string.Join(",", requestedLenochClassificationIds)));
        }

        return requestedLenochClassifications;
    }

    public async Task<IList<Subject>> ValidateSubjects(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.SubjectIds == null || !entryModification.SubjectIds.Any())
        {
            return new List<Subject>();
        }

        var groupedIds = entryModification.SubjectIds.GroupBy(x => x);

        if (groupedIds.Any(id => id.Count() > 1))
        {
            throw new BusinessValidationException(nameof(entryModification.SubjectIds), BackendTexts.EntrySubjectsDuplicates);
        }

        var requestedSubjects = (await subjectRepository.GetByIds(entryModification.SubjectIds)).ToList();
        var requestedSubjectsIds = entryModification.SubjectIds.OrderBy(id => id).ToArray();
        var loadedSubjectIds = requestedSubjects.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedSubjectsIds.SequenceEqual(loadedSubjectIds))
        {
            throw new BusinessValidationException(nameof(entryModification.SubjectIds), string.Format(CultureInfo.InvariantCulture, BackendTexts.EntrySubjectsNotFound, string.Join(",", entryModification.SubjectIds)));
        }

        return requestedSubjects;
    }

    public async Task<IList<Domain.Models.Domain>> ValidateDomains(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.DomainIds == null || !entryModification.DomainIds.Any())
        {
            return new List<Domain.Models.Domain>();
        }

        var requestedDomains = (await domainRepository.GetByIds(entryModification.DomainIds)).ToList();
        var requestedDomainIds = entryModification.DomainIds.OrderBy(id => id).ToArray();
        var loadedDomainIds = requestedDomains.OrderBy(rd => rd.Id).Select(rd => rd.Id).ToArray();

        if (!requestedDomainIds.SequenceEqual(loadedDomainIds))
        {
            throw new BusinessValidationException(nameof(entryModification.DomainIds), string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryDomainsNotFound, string.Join(",", entryModification.DomainIds)));
        }

        var requestedDomainSubjectIds = requestedDomains.Select(rd => rd.SubjectId).Distinct().ToList();

        if (!requestedDomainSubjectIds.TrueForAll(id => entryModification.SubjectIds.Contains(id)))
        {
            var invalidDomains = requestedDomainSubjectIds.Where(id => !entryModification.SubjectIds.Contains(id)).ToList();
            throw new BusinessValidationException(nameof(entryModification.DomainIds), string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryDomainsInvalid, string.Join(",", invalidDomains)));
        }

        return requestedDomains;
    }

    public async Task<IList<Collection>> ValidateRelatedCollections(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.RelatedCollectionIds == null || !entryModification.RelatedCollectionIds.Any())
        {
            return new List<Collection>();
        }

        var requestedCollections = (await collectionRepository.GetByIds(entryModification.RelatedCollectionIds))
            .ToList();

        var requestedCollectionIds = entryModification.RelatedCollectionIds.OrderBy(id => id).ToArray();
        var loadedCollectionIds = requestedCollections.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedCollectionIds.SequenceEqual(loadedCollectionIds))
        {
            throw new BusinessValidationException(
                nameof(entryModification.RelatedCollectionIds),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryRelatedCollectionsNotFound, string.Join(",", requestedCollectionIds)));
        }

        return requestedCollections;
    }

    public async Task<ICollection<Glossary>> ValidateGlossaries(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.GlossaryIds == null || !entryModification.GlossaryIds.Any())
        {
            return new List<Glossary>();
        }

        var requestedGlossaries = (await glossaryRepository.GetByIds(entryModification.GlossaryIds))
            .ToList();

        var requestedGlossaryIds = entryModification.GlossaryIds.OrderBy(id => id).ToArray();
        var loadedGlossaryIds = requestedGlossaries.OrderBy(rc => rc.Id).Select(rc => rc.Id).ToArray();
        if (!requestedGlossaryIds.SequenceEqual(loadedGlossaryIds))
        {
            throw new BusinessValidationException(
                nameof(entryModification.GlossaryIds),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryGlossariesNotFound, string.Join(",", requestedGlossaryIds)));
        }

        return requestedGlossaries;
    }

    public void AuthorizeGlossaries(IEnumerable<int> glossaryIds, IEnumerable<Glossary> existingGlossaries = null)
    {
        if (glossaryIds == null || authorizationService.IsAdmin() || authorizationService.IsSuperUser())
        {
            return;
        }

        var availableGlossaries = authorizationService.GetAuthorizedGlossaryIds().ToList();

        if (existingGlossaries != null)
        {
            availableGlossaries.AddRange(existingGlossaries.Select(x => x.Id));
        }

        if (glossaryIds.Any(id => !availableGlossaries.Contains(id)))
        {
            throw new ForbiddenException(BackendTexts.Unauthorized);
        }
    }

    public async Task<ICollection<EntryReference>> ValidateReferences(IEntryModification entryModification, Entry clientEntry)
    {
        ArgumentNullException.ThrowIfNull(entryModification);
        ArgumentNullException.ThrowIfNull(clientEntry);

        if (entryModification.References == null || !entryModification.References.Any())
        {
            return new List<EntryReference>();
        }
        var requestedEntryIds = entryModification.References
            .Select(emr => emr.EntryId)
            .OrderBy(id => id)
            .Distinct()
            .ToArray();

        var requestedEntries = (await entryRepository.GetByIds(requestedEntryIds))
            .ToList();

        var loadedEntryIds = requestedEntries
            .OrderBy(rc => rc.Id)
            .Select(rc => rc.Id)
            .ToArray();

        if (!requestedEntryIds.SequenceEqual(loadedEntryIds))
        {
            throw new BusinessValidationException(
                nameof(entryModification.References),
                string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryReferencesNotFound, string.Join(",", requestedEntryIds)));
        }

        foreach (var entryReference in clientEntry.References)
        {
            entryReference.EntryId = clientEntry.Id;
        }

        return clientEntry.References;
    }

    public ICollection<EntryExternalLink> ValidateExternalLinks(IEntryModification entryModification, Entry clientEntry)
    {
        ArgumentNullException.ThrowIfNull(entryModification);
        ArgumentNullException.ThrowIfNull(clientEntry);

        if (entryModification.ExternalLinks == null || !entryModification.ExternalLinks.Any())
        {
            return new List<EntryExternalLink>();
        }

        foreach (var entryExternalLink in clientEntry.ExternalLinks)
        {
            var validUrl =
                Uri.TryCreate(entryExternalLink.Url, UriKind.Absolute, out var uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!validUrl)
            {
                throw new BusinessValidationException(
                    nameof(entryModification.ExternalLinks),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.ExternalLinkInvalid, entryExternalLink.Url));
            }

            entryExternalLink.EntryId = clientEntry.Id;
        }

        return clientEntry.ExternalLinks;
    }

    public async Task<Picture> ValidatePicture(Picture clientPicture)
    {
        if (clientPicture == null)
        {
            return null;
        }

        var languages = (await languageRepository.GetAll()).ToList();

        foreach (var pictureTranslation in clientPicture.Translations)
        {
            try
            {
                pictureTranslation.Language = languages.Single(l => l.Id == pictureTranslation.LanguageId);
            }
            catch (InvalidOperationException)
            {
                throw new BusinessValidationException(
                    nameof(pictureTranslation.Language),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryLanguageNotFound, pictureTranslation.LanguageId));
            }
        }

        return clientPicture;
    }

    public async Task<IList<AdminInfo>> ValidateAdminInfos(IEntryModification entryModification)
    {
        ArgumentNullException.ThrowIfNull(entryModification);

        if (entryModification.AdminInfos == null || !entryModification.AdminInfos.Any())
        {
            return new List<AdminInfo>();
        }

        var allCodes = entryModification.AdminInfos.Select(a => a.Code).ToList();

        var allAdminInfos = (await adminInfoRepository.GetByCodes(allCodes)).ToList();

        return allAdminInfos;
    }

    private static string EncodeNonEmptyHyperlinks(string source)
    {
        return !string.IsNullOrEmpty(source) ? EscapeHyperlinksInString(source) : null;
    }

    private static string EscapeHyperlinksInString(string markDownString)
    {
        var regex = new Regex(@"\[([^\]]+)]\(([^)]+)\)", RegexOptions.None, TimeSpan.FromSeconds(1));
        var matchesWithHyperLink = regex.Matches(markDownString);
        var escapedString = markDownString;
        if (matchesWithHyperLink.Count > 0)
        {
            foreach (Match match in matchesWithHyperLink)
            {
                var hyperlinkGroup = match.Groups[2];
                if (hyperlinkGroup.Value.Contains('(', StringComparison.InvariantCulture))
                {
                    var hyperlink = GetHyperlinkWithBrackets(markDownString, hyperlinkGroup.Index - 1);
                    if (hyperlink is null)
                    {
                        continue;
                    }
                    var escapedHyperlink = EscapeBrackets(hyperlink);
                    escapedString = escapedString.Replace(hyperlink, escapedHyperlink, StringComparison.InvariantCulture);
                }
            }
        }

        return escapedString;
    }

    private static string EscapeBrackets(string input)
    {
        var sb = new StringBuilder();
        foreach (var c in input)
        {
            switch (c)
            {
                case '(':
                    sb.Append(Uri.HexEscape('('));
                    break;
                case ')':
                    sb.Append(Uri.HexEscape(')'));
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    private static string GetHyperlinkWithBrackets(string source, int start)
    {
        var subStringWithLink = source[start..];
        var openBrackets = 0;
        foreach (var c in subStringWithLink.Select((character, index) => (character, index)))
        {
            if (c.character == '(')
            {
                openBrackets++;
            }
            else if (c.character == ')')
            {
                openBrackets--;
                if (openBrackets == 0)
                {
                    return source.Substring(start + 1, c.index - 1);
                }
            }
        }

        return null;
    }
}
