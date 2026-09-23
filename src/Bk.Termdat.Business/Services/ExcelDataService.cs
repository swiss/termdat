using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bk.DocumentService.Client;
using Bk.DocumentService.Client.Models;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Bk.Utils.Models;
using Bk.Utils.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExternalLinkType = Bk.Termdat.Business.Domain.Models.ExternalLinkType;
using ReferenceType = Bk.Termdat.Business.Domain.Models.ReferenceType;
using Reliability = Bk.Termdat.Business.Resource.Models.Reliability;
using Status = Bk.Termdat.Business.Resource.Models.Status;

namespace Bk.Termdat.Business.Services;

public class ExcelDataService : IExcelDataService
{
    // Note:
    // Excel column indices (1-based, as used by ClosedXML).
    // Column 2 (EntryId) is currently unused and reserved for future export/import update functionality.
    private const int ColGroupKey = 1;

    // private const int ColEntryId = 2;
    private const int ColSequence = 3;
    private const int ColLanguage = 4;
    private const int ColName = 5;
    private const int ColNameSource = 6;
    private const int ColTerminus = 7;
    private const int ColTerminusSource = 8;
    private const int ColPhraseology = 9;
    private const int ColPhraseologySource = 10;
    private const int ColAbbreviation = 11;
    private const int ColAbbreviationSource = 12;
    private const int ColDefinition = 13;
    private const int ColDefinitionSource = 14;
    private const int ColNote = 15;
    private const int ColNoteSource = 16;
    private const int ColContext = 17;
    private const int ColContextSource = 18;
    private const int ColMetadata = 19;
    private const int ColMetadataSource = 20;
    private const int ColCountry = 21;
    private readonly ICultureService cultureService;
    private readonly IDocumentService documentService;
    private readonly EmailOptions emailOptions;
    private readonly IEmailService emailService;
    private readonly IEntryRepository entryRepository;
    private readonly IEntryService entryService;
    private readonly FileImportOptions fileImportOptions;
    private readonly ILanguageRepository languageRepository;
    private readonly ILogger<ExcelDataService> logger;
    private readonly ITermdatUrlService termdatUrlService;
    private readonly IList<KeyValuePair<string, string>> validationErrors = new List<KeyValuePair<string, string>>();

    public ExcelDataService(
        IOptions<FileImportOptions> fileImportOptions,
        IOptions<EmailOptions> emailOptions,
        ILogger<ExcelDataService> logger,
        ILanguageRepository languageRepository,
        IEntryService entryService,
        IEmailService emailService,
        IDocumentService documentService,
        IEntryRepository entryRepository,
        ICultureService cultureService,
        ITermdatUrlService termdatUrlService)
    {
        this.fileImportOptions = fileImportOptions.Value;
        this.emailOptions = emailOptions.Value;
        this.logger = logger;
        this.languageRepository = languageRepository;
        this.entryService = entryService;
        this.emailService = emailService;
        this.documentService = documentService;
        this.entryRepository = entryRepository;
        this.cultureService = cultureService;
        this.termdatUrlService = termdatUrlService;
    }

    public async Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, XLWorkbook workbook)>
        IsFileValid(IFormFile file)
    {
        try
        {
            validationErrors.Clear();

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var workbook = new XLWorkbook(memoryStream);

            if (workbook.Worksheets.Count == 0)
            {
                validationErrors.Add(new KeyValuePair<string, string>("error", "Excel file contains no worksheets"));
                return (false, validationErrors, null);
            }

            var worksheet = workbook.Worksheet(1);
            if (worksheet.LastRowUsed() == null || worksheet.LastRowUsed()?.RowNumber() < 2)
            {
                validationErrors.Add(new KeyValuePair<string, string>("error",
                    "Excel file must contain at least a header row and one data row"));
                return (false, validationErrors, null);
            }

            // Validate required columns exist (at least 21 columns)
            if (worksheet.LastColumnUsed() == null || worksheet.LastColumnUsed()?.ColumnNumber() < 21)
            {
                validationErrors.Add(new KeyValuePair<string, string>("error",
                    "Excel file must contain all required columns (at least 21 columns)"));
                return (false, validationErrors, null);
            }

            return (!validationErrors.Any(), validationErrors, workbook);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during Excel file validation attempt");
            validationErrors.Add(new KeyValuePair<string, string>("error",
                "Unexpected error during Excel file validation attempt"));
            return (false, validationErrors, null);
        }
    }

    public async Task ImportDocument(XLWorkbook workbook, string importJobLabel, string currentUserEmail,
        int entryOfficeId, int entryCollectionId, Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos,
        string comment)
    {
        try
        {
            adminInfos.Add(new AdminInfoDetail { Code = importJobLabel });

            var entryCreateItems = await ComposeEntryCreateItems(workbook, entryOfficeId, entryCollectionId,
                status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);

            foreach (var entryCreateItem in entryCreateItems)
            {
                await entryService.Create(entryCreateItem, fileImportOptions.Email);
            }

            var mailBody = string.Format(BackendTexts.FileImportMailSuccess, importJobLabel, entryCreateItems.Count, 0);
            await SendEmailAsync(mailBody);
        }
        catch (ForbiddenException)
        {
            var mailBody = string.Format(BackendTexts.FileImportMailError, importJobLabel,
                "Missing permissions for some or all included glossaries or collections.");
            await SendEmailAsync(mailBody);
        }
        catch (BusinessValidationException businessValidationException)
        {
            var validationExceptionMessage =
                $"Validation exception: {businessValidationException.ModelKey ?? "unknown"}: {businessValidationException.Message}";
            var mailBody = string.Format(BackendTexts.FileImportMailError, importJobLabel, validationExceptionMessage);
            await SendEmailAsync(mailBody);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error during import");
            var mailBody = string.Format(BackendTexts.FileImportMailError, importJobLabel,
                "Unexpected error during import.");
            await SendEmailAsync(mailBody);
        }

        async Task SendEmailAsync(string message)
        {
            try
            {
                var email = new Email
                {
                    From = new EmailAddress
                    {
                        Name = emailOptions.SenderAddress,
                        Address = emailOptions.SenderAddress
                    },
                    To = [new EmailAddress { Name = currentUserEmail, Address = currentUserEmail }],
                    Subject = importJobLabel,
                    HtmlMessage = message
                };

                await emailService.Send(email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Excel Import - last life sign: {ImportJobLabel} - {CurrentUserEmail} - Could not send email. Therefore, the user has no information about the current status of the import. This terminates the current import process",
                    importJobLabel, currentUserEmail);
            }
        }
    }

    public async Task<(string fileName, Stream content)> CreateExport(IReadOnlyList<int> ids)
    {
        var entries = await entryRepository.GetByIdsForExport(ids);

        var spreadsheet = new Spreadsheet();

        AddHeaders(spreadsheet);

        foreach (var entry in entries)
        {
            var languageGroups = entry.Translations
                .OrderBy(t => t.Language.SortIndex)
                .ThenBy(t => t.Sequence)
                .GroupBy(t => t.Language);

            foreach (var languageGroup in languageGroups)
            {
                foreach (var entryLanguage in languageGroup)
                {
                    var entryUrl = termdatUrlService.GetTermdatUrl(entry.Id);

                    var row = new List<Cell>
                    {
                        new() { Text = entry.Id.ToString() },
                        new() { Text = entryLanguage.Sequence.ToString(), FormatType = CellFormatTypes.Number },
                        new() { Text = entryLanguage.Language?.IsoCode?.ToUpper() ?? string.Empty },
                        new() { Text = entryUrl, Hyperlink = entryUrl },
                        new()
                        {
                            Text = GetTextWithCode(entry.Office?.Code,
                                cultureService.GetTranslation(entry.Office?.Translations))
                        },
                        new()
                        {
                            Text = GetTextWithCode(entry.Collection?.Code,
                                cultureService.GetTranslation(entry.Collection?.Translations))
                        },
                        new() { Text = StatusToText(entry.Status) },
                        new() { Text = ReliabilityToText(entry.Reliability) },
                        new() { Text = GetSubjectsAndDomainsText(entry) },
                        new() { Text = GetMultiValueText(entry.RelatedCollections) },
                        new() { Text = GetMultiValueText(entry.Glossaries) },
                        new() { Text = entry.Comment ?? string.Empty },
                        new() { Text = string.Join("|", entry.AdminInfos?.Select(a => a.Code) ?? []) },
                        new() { Text = GetOutgoingReferencesText(entry.References) },
                        new() { Text = GetExternalLinksText(entry.ExternalLinks) },
                        new() { Text = entry.ConceptSystemPosition?.Value ?? string.Empty },
                        new()
                        {
                            Text = entry.Created.ToString("O"),
                            FormatType = CellFormatTypes.Date,
                            Format = "dd.MM.yyyy HH:mm:ss"
                        },
                        new()
                        {
                            Text = entry.Modified.ToString("O"),
                            FormatType = CellFormatTypes.Date,
                            Format = "dd.MM.yyyy HH:mm:ss"
                        },
                        new() { Text = entryLanguage.Name ?? string.Empty },
                        new() { Text = entryLanguage.NameSource ?? string.Empty },
                        new() { Text = entryLanguage.Terminus ?? string.Empty },
                        new() { Text = entryLanguage.TerminusSource ?? string.Empty },
                        new() { Text = entryLanguage.Phraseology ?? string.Empty },
                        new() { Text = entryLanguage.PhraseologySource ?? string.Empty },
                        new() { Text = entryLanguage.Abbreviation ?? string.Empty },
                        new() { Text = entryLanguage.AbbreviationSource ?? string.Empty },
                        new() { Text = entryLanguage.Definition ?? string.Empty },
                        new() { Text = entryLanguage.DefinitionSource ?? string.Empty },
                        new() { Text = entryLanguage.Note ?? string.Empty },
                        new() { Text = entryLanguage.NoteSource ?? string.Empty },
                        new() { Text = entryLanguage.Context ?? string.Empty },
                        new() { Text = entryLanguage.ContextSource ?? string.Empty },
                        new() { Text = entryLanguage.Metadata ?? string.Empty },
                        new() { Text = entryLanguage.MetadataSource ?? string.Empty },
                        new() { Text = entryLanguage.Country ?? string.Empty }
                    };

                    spreadsheet.BodyCells.Add(row);
                }
            }
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fileName = $"Export-{timestamp}.xlsx";
        var stream = await documentService.CreateExcel(spreadsheet);

        return (fileName, stream);
    }

    public async Task<(string fileName, Stream content)> CreateDuplicateCheckExport(IList<DuplicateCheckMatch> matches)
    {
        var spreadsheet = new Spreadsheet();

        spreadsheet.HeaderCells.Add(CreateHeaderCell("DuplicateCheck.Export.XLSX.Header.EntryId"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("DuplicateCheck.Export.XLSX.Header.MatchText"));

        var matchedEntryIdCount = matches.Any() ? matches.Max(x => x.MatchedEntryIds.Length) : 0;
        for (var i = 0; i < matchedEntryIdCount; i++)
        {
            spreadsheet.HeaderCells.Add(CreateHeaderCell("DuplicateCheck.Export.XLSX.Header.DuplicateEntryId"));
        }

        foreach (var match in matches)
        {
            var row = new List<Cell>
            {
                new() { Text = match.EntryId.ToString() },
            };

            var exportKeys = match.MatchedKeys
                .SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(x => x.Length)
                .ToList();

            var filteredKeys = new List<string>();

            foreach (var key in exportKeys)
            {
                if (!filteredKeys.Any(existing =>
                    existing.Contains(key, StringComparison.OrdinalIgnoreCase)))
                {
                    filteredKeys.Add(key);
                }
            }

            row.Add(new Cell
            {
                Text = string.Join(", ", exportKeys)
            });

            foreach (var id in match.MatchedEntryIds)
            {
                row.Add(new() { Text = id.ToString() });
            }

            spreadsheet.BodyCells.Add(row);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd");
        var fileName = $"{timestamp}_{BackendTexts.ResourceManager.GetString("DuplicateCheck.Export.XLSX.FileName")}.xlsx";
        var stream = await documentService.CreateExcel(spreadsheet);

        return (fileName, stream);
    }

    private static void AddHeaders(Spreadsheet spreadsheet)
    {
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.EntryId"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Sequence"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Language"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.EntryLink"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Office"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Collection"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Status"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Reliability"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.SubjectsAndDomains"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.RelatedCollections"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Glossaries"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Comment"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.AdminInfos"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.OutgoingReferences"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.ExternalLinks"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.ConceptSystemPosition"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Created"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Modified"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Name"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.NameSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Terminus"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.TerminusSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Phraseology"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.PhraseologySource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Abbreviation"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.AbbreviationSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Definition"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.DefinitionSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Note"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.NoteSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Context"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.ContextSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Metadata"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.MetadataSource"));
        spreadsheet.HeaderCells.Add(CreateHeaderCell("Watchlist.Export.XLSX.Header.Country"));
        return;
    }

    private static Cell CreateHeaderCell(string resourceKey) => new()
    {
        Text = BackendTexts.ResourceManager.GetString(resourceKey),
        Format = CellFormat.Bold
    };

    private static string StatusToText(Domain.Models.Status status)
    {
        return status == Domain.Models.Status.Validated ? BackendTexts.StatusValidated : BackendTexts.StatusInProgress;
    }

    private static string ReliabilityToText(Domain.Models.Reliability reliability)
    {
        var text = reliability switch
        {
            Domain.Models.Reliability.FirstDraft => BackendTexts.ReliabilityFirstDraft,
            Domain.Models.Reliability.Tentative => BackendTexts.ReliabilityTentative,
            Domain.Models.Reliability.Translated => BackendTexts.ReliabilityTranslated,
            Domain.Models.Reliability.Verified => BackendTexts.ReliabilityVerified,
            Domain.Models.Reliability.Approved => BackendTexts.ReliabilityApproved,
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(text) ? string.Empty : $"{(int)reliability + 1} - {text}";
    }

    private string GetSubjectsAndDomainsText(Entry entry)
    {
        if (entry.Subjects == null || entry.Subjects.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>();

        foreach (var subject in entry.Subjects.OrderBy(s => s.Id))
        {
            var subjectName = cultureService.GetTranslation(subject.Translations);
            var domainsForSubject = entry.Domains?.Where(d => d.SubjectId == subject.Id).ToList();

            if (domainsForSubject != null && domainsForSubject.Count != 0)
            {
                var domainNames = domainsForSubject
                    .OrderBy(d => d.Id)
                    .Select(d => cultureService.GetTranslation(d.Translations))
                    .Where(name => !string.IsNullOrWhiteSpace(name));

                lines.Add($"{subjectName}||{string.Join("|", domainNames)}");
            }
            else
            {
                lines.Add(subjectName);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string GetMultiValueText(IEnumerable<dynamic> items)
    {
        var itemsList = items?.ToList();
        if (itemsList == null || itemsList.Count == 0)
        {
            return string.Empty;
        }

        var values = new List<string>();
        foreach (var item in itemsList)
        {
            string translation = cultureService.GetTranslation(item.Translations);
            if (!string.IsNullOrWhiteSpace(translation))
            {
                var value = !string.IsNullOrWhiteSpace(item.Code)
                    ? $"{item.Code} - {translation}"
                    : translation;
                values.Add(value);
            }
        }

        return string.Join("|", values);
    }

    private static string GetTextWithCode(string code, string translation)
    {
        return string.IsNullOrWhiteSpace(translation) ? string.Empty :
            !string.IsNullOrWhiteSpace(code) ? $"{code} - {translation}" : translation;
    }

    private static string GetOutgoingReferencesText(ICollection<EntryReference> references)
    {
        if (references == null || references.Count == 0)
        {
            return string.Empty;
        }

        var items = references
            .Where(r => !r.ReferencedEntry.Deleted)
            .Select(r => new { r.Type, EntryId = r.ReferencedEntryId })
            .OrderBy(x => x.Type)
            .ThenBy(x => x.EntryId);

        var referenceStrings = items.Select(item =>
        {
            var typeCode = item.Type switch
            {
                ReferenceType.General => BackendTexts.ReferenceTypeGeneralCode,
                ReferenceType.Specific => BackendTexts.ReferenceTypeSpecificCode,
                ReferenceType.Antonym => BackendTexts.ReferenceTypeAntonymCode,
                ReferenceType.Whole => BackendTexts.ReferenceTypeWholeCode,
                ReferenceType.Part => BackendTexts.ReferenceTypePartCode,
                _ => string.Empty
            };

            return $"{typeCode},{item.EntryId}";
        });

        return string.Join("|", referenceStrings);
    }

    private static string GetExternalLinksText(ICollection<EntryExternalLink> externalLinks)
    {
        if (externalLinks == null || externalLinks.Count == 0)
        {
            return string.Empty;
        }

        var linkStrings = externalLinks
            .OrderBy(l => l.Type)
            .ThenBy(l => l.Url)
            .Select(link =>
            {
                var typeText = link.Type switch
                {
                    ExternalLinkType.Wikidata => Enum.GetName(ExternalLinkType.Wikidata),
                    ExternalLinkType.Fedlex => Enum.GetName(ExternalLinkType.Fedlex),
                    ExternalLinkType.Lindas => Enum.GetName(ExternalLinkType.Lindas),
                    ExternalLinkType.None => Enum.GetName(ExternalLinkType.None),
                    _ => string.Empty
                };

                return $"{typeText},{link.Url}";
            });

        return string.Join("|", linkStrings);
    }

    protected async Task<IList<EntryCreate>> ComposeEntryCreateItems(XLWorkbook workbook, int entryOfficeId,
        int entryCollectionId,
        Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos,
        string comment)
    {
        var result = new List<EntryCreate>();
        var languages = await GetLanguages();
        var worksheet = workbook.Worksheet(1);

        // Skip header row
        var dataRows = worksheet.RowsUsed().Skip(1);
        var groupedRows = dataRows.GroupBy(row => GetCellValue(row, ColGroupKey));

        foreach (var group in groupedRows)
        {
            var groupKey = group.Key;
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                continue; // Skip rows without GroupKey
            }

            var entryCreate = new EntryCreate
            {
                OfficeId = entryOfficeId,
                CollectionId = entryCollectionId,
                Status = status,
                Reliability = reliability,
                SubjectIds = subjectIds,
                DomainIds = domainIds,
                GlossaryIds = glossaryIds,
                AdminInfos = adminInfos,
                Comment = comment,
                LanguageDetails = new List<EntryModificationLanguageDetail>()
            };

            foreach (var row in group.OrderBy(r => GetCellValueAsInt(r, ColSequence)))
            {
                var rowNumber = row.RowNumber();
                var languageDetail = new EntryModificationLanguageDetail();

                var sequence = GetCellValueAsInt(row, ColSequence);
                if (sequence == 0)
                {
                    throw new BusinessValidationException("Sequence",
                        $"Row {rowNumber}: SequenceNumber is required and must be greater than 0");
                }

                languageDetail.Sequence = sequence;

                var language = GetCellValue(row, ColLanguage);
                if (!TryGetLanguageId(languages, language, out var languageId))
                {
                    throw new BusinessValidationException("Language",
                        $"Row {rowNumber}: Language '{language}' is missing or not supported");
                }

                languageDetail.LanguageId = languageId;

                languageDetail.Name = GetCellValue(row, ColName);
                languageDetail.NameSource = GetCellValue(row, ColNameSource);
                languageDetail.Terminus = GetCellValue(row, ColTerminus);
                languageDetail.TerminusSource = GetCellValue(row, ColTerminusSource);
                languageDetail.Phraseology = GetCellValue(row, ColPhraseology);
                languageDetail.PhraseologySource = GetCellValue(row, ColPhraseologySource);
                languageDetail.Abbreviation = GetCellValue(row, ColAbbreviation);
                languageDetail.AbbreviationSource = GetCellValue(row, ColAbbreviationSource);

                // Definition - only set on the first synonym (sequence == 1)
                if (sequence == 1)
                {
                    languageDetail.Definition = GetCellValue(row, ColDefinition);
                    languageDetail.DefinitionSource = GetCellValue(row, ColDefinitionSource);
                }

                languageDetail.Note = GetCellValue(row, ColNote);
                languageDetail.NoteSource = GetCellValue(row, ColNoteSource);
                languageDetail.Context = GetCellValue(row, ColContext);
                languageDetail.ContextSource = GetCellValue(row, ColContextSource);
                languageDetail.Metadata = GetCellValue(row, ColMetadata);
                languageDetail.MetadataSource = GetCellValue(row, ColMetadataSource);
                languageDetail.Country = GetCellValue(row, ColCountry);

                entryCreate.LanguageDetails.Add(languageDetail);
            }

            result.Add(entryCreate);
        }

        return result;
    }

    private static string GetCellValue(IXLRow row, int columnIndex)
    {
        var cell = row.Cell(columnIndex);
        return cell.IsEmpty() ? null : cell.GetValue<string>()?.Trim();
    }

    private static int GetCellValueAsInt(IXLRow row, int columnIndex)
    {
        var value = GetCellValue(row, columnIndex);
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return int.TryParse(value, out var result) ? result : 0;
    }

    private static bool TryGetLanguageId(IList<LanguageItem> availableLanguages, string languageValue, out int result)
    {
        result = -1;

        if (string.IsNullOrWhiteSpace(languageValue))
        {
            return false;
        }

        // Only accept two-letter ISO codes (case-insensitive)
        var languageId = availableLanguages.FirstOrDefault(languageItem =>
                languageItem.TwoLetterIsoCode.Equals(languageValue, StringComparison.InvariantCultureIgnoreCase))
            ?.LanguageId;

        if (!languageId.HasValue)
        {
            return false;
        }

        result = languageId.Value;
        return true;
    }

    private async Task<IList<LanguageItem>> GetLanguages()
    {
        var languages = await languageRepository.GetAll();
        return languages.Select(l => new LanguageItem { LanguageId = l.Id, TwoLetterIsoCode = l.IsoCode }).ToList();
    }

    private record LanguageItem
    {
        public int LanguageId { get; init; }
        public string TwoLetterIsoCode { get; init; }
    }
}
