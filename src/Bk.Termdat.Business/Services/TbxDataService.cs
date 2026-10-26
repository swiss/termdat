using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swiss.FCh.Sparql.Extensions;
using Swiss.FCh.Utils.Models;
using Swiss.FCh.Utils.Services;

namespace Bk.Termdat.Business.Services;

public class TbxDataService : ITbxDataService
{
    private const string XmlSchemasBaseNamespace = "Bk.Termdat.Business.Resource.XmlSchemas.";
    private readonly IEntryRepository entryRepository;
    private readonly IEntryService entryService;
    private readonly IEmailService emailService;
    private readonly ILanguageRepository languageRepository;
    private readonly ILogger<TbxDataService> logger;
    private readonly FileImportOptions fileImportOptions;
    private readonly EmailOptions emailOptions;
    private readonly List<KeyValuePair<string, string>> validationErrors = new();

    public TbxDataService(
        IOptions<FileImportOptions> importOptions,
        ILogger<TbxDataService> logger,
        ILanguageRepository languageRepository,
        IEntryRepository entryRepository,
        IEntryService entryService,
        IEmailService emailService,
        IOptions<EmailOptions> emailOptions)
    {
        ArgumentNullException.ThrowIfNull(importOptions);
        ArgumentNullException.ThrowIfNull(emailOptions);

        fileImportOptions = importOptions.Value;
        this.logger = logger;
        this.languageRepository = languageRepository;
        this.entryRepository = entryRepository;
        this.entryService = entryService;
        this.emailService = emailService;
        this.emailOptions = emailOptions.Value;
    }

    public async Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, XDocument document)>
        IsFileValid(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        //Note: The current validation uses a TBX core schema for TBX 3.0 files to validate the given XML. And it also validates the base XML structure.
        //      Based on recent information (February 2024), Star (the company where the export is implemented) is exporting in an older TBX 2.0 format.
        //      By the end of 2024, the export should be able to provide TBX 3.0.
        //      Therefore, the given validation remains in place.
        try
        {
            validationErrors.Clear();
            var executingAssembly = Assembly.GetExecutingAssembly();
            var schemaSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var tbxCoreSchemaStream = executingAssembly.GetManifestResourceStream($"{XmlSchemasBaseNamespace}TBX_core.xsd")
                ?? throw new InvalidOperationException("TBX_core.xsd resource not found.");

            using var tbxCoreSchemaReader = XmlReader.Create(tbxCoreSchemaStream, schemaSettings);

            var tbxCoreSchema = XmlSchema.Read(tbxCoreSchemaReader, ValidationEventHandler);

            using var baseXmlSchemaStream = executingAssembly.GetManifestResourceStream($"{XmlSchemasBaseNamespace}Xml.xsd")
                ?? throw new InvalidOperationException("Xml.xsd resource not found.");

            using var baseXmlSchemaReader = XmlReader.Create(baseXmlSchemaStream, schemaSettings);

            var baseXmlSchema = XmlSchema.Read(baseXmlSchemaReader, ValidationEventHandler);

            var xmlSchemaImport =
                new XmlSchemaImport { Schema = baseXmlSchema, Namespace = "http://www.w3.org/XML/1998/namespace" };
            tbxCoreSchema.Includes.Add(xmlSchemaImport);

            var xmlReaderSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                DtdProcessing = DtdProcessing.Parse,
                ConformanceLevel = ConformanceLevel.Document
            };
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
            xmlReaderSettings.Schemas.Add(tbxCoreSchema);

            using var memoryStream = await ReadAsSanitizedXmlMemoryStreamAsync(file);
            using var fileXmlReader = XmlReader.Create(memoryStream, xmlReaderSettings);
            var document = XDocument.Load(fileXmlReader, LoadOptions.SetLineInfo);

            return (validationErrors.Count == 0, validationErrors, document);
        }
        catch (XmlException xmlException)
        {
            logger.LogError(xmlException, "Unexpected error during TBX file validation attempt");
            validationErrors.Add(new KeyValuePair<string, string>("error", xmlException.Message));
            return (false, validationErrors, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during TBX file validation attempt");
            validationErrors.Add(new KeyValuePair<string, string>("error",
                "Unexpected error during TBX file validation attempt"));
            return (false, validationErrors, null);
        }
    }

    public async Task ImportDocument(XDocument document, string importJobLabel, string currentUserEmail,
        int entryOfficeId, int entryCollectionId, Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds, IList<AdminInfoSummary> adminInfos,
        string comment)
    {
        ArgumentNullException.ThrowIfNull(adminInfos);

        try
        {
            adminInfos.Add(new AdminInfoDetail { Code = importJobLabel });

            var modificationItems = await ComposeEntryModificationItems(document, entryOfficeId, entryCollectionId,
                status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);

            var createdItemsCount = 0;
            var updatedItemsCount = 0;
            foreach (var modificationItem in modificationItems)
            {
                switch (modificationItem)
                {
                    case EntryCreate entryCreateItem:
                    {
                        createdItemsCount++;
                        await entryService.Create(entryCreateItem, fileImportOptions.Email);
                        break;
                    }
                    case EntryUpdate entryUpdateItem:
                    {
                        updatedItemsCount++;
                        await entryService.Update(entryUpdateItem, processingUser: fileImportOptions.Email);
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Unexpected modification item type: {modificationItem?.GetType().FullName ?? "null"}");
                    }
                }
            }

            var mailBody = string.Format(CultureInfo.InvariantCulture, BackendTexts.FileImportMailSuccess, importJobLabel, createdItemsCount,
                updatedItemsCount);
            await SendEmailAsync(mailBody);
        }
        catch (ForbiddenException)
        {
            var mailBody = string.Format(CultureInfo.InvariantCulture, BackendTexts.FileImportMailError, importJobLabel,
                "Missing permissions for some or all included glossaries or collections.");
            await SendEmailAsync(mailBody);
        }
        catch (BusinessValidationException businessValidationException)
        {
            var validationExceptionMessage =
                $"Validation exception: {businessValidationException.ModelKey ?? "unknown"}: {businessValidationException.Message}";
            var mailBody = string.Format(CultureInfo.InvariantCulture, BackendTexts.FileImportMailError, importJobLabel, validationExceptionMessage);
            await SendEmailAsync(mailBody);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error during import");
            var mailBody = string.Format(CultureInfo.InvariantCulture, BackendTexts.FileImportMailError, importJobLabel,
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
                // Note:
                // If email communication fails, meaning the user cannot be reached in any way,
                // this log entry is the last sign of life of the current import process.
                logger.LogError(ex,
                    "TBX Import - last life sign: {ImportJobLabel} - {CurrentUserEmail} - Could not send email. Therefore, the user has no information about the current status of the import. This terminates the current import process",
                    importJobLabel, currentUserEmail);
            }
        }
    }

    protected async Task<IList<IEntryModification>> ComposeEntryModificationItems(XDocument document, int entryOfficeId,
        int entryCollectionId,
        Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds, IList<AdminInfoSummary> adminInfos,
        string comment)
    {
        ArgumentNullException.ThrowIfNull(document);

        var result = new List<IEntryModification>();
        var languages = await GetLanguages();

        var termEntries = document.Descendants("termEntry");

        foreach (var termEntry in termEntries)
        {
            if (!Guid.TryParse(termEntry.Descendants("admin")
                    .SingleOrDefault(admin => admin.Attribute("type")?.Value == "GUID")?.Value, out var termEntryGuid))
            {
                var (lineNumber, linePosition) = GetLineInfos(termEntry);
                throw new BusinessValidationException("termEntry",
                    $"{ComposeLineInfo(lineNumber, linePosition)} Missing or invalid guid on term entry");
            }

            var externalSystemId = termEntryGuid.ToString();
            var existingEntry = await entryRepository.GetByExternalSystemId(externalSystemId);

            IEntryModification entryModification;
            if (existingEntry is null)
            {
                entryModification = new EntryCreate
                {
                    ExternalSystemId = externalSystemId,
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
            }
            else
            {
                var modificationDate = termEntry.Elements("transacGrp").SingleOrDefault(transacGrp =>
                        transacGrp.Elements("transac").SingleOrDefault(transac =>
                            transac.Attribute("type")?.Value == "transactionType")?.Value == "modification")
                    ?.Element("date")?.Value;

                if (!DateTime.TryParse(modificationDate, CultureInfo.InvariantCulture, out var termEntryModificationDate))
                {
                    var (lineNumber, linePosition) = GetLineInfos(termEntry);
                    throw new BusinessValidationException("termEntry",
                        $"{ComposeLineInfo(lineNumber, linePosition)} Missing or invalid modification date on term entry");
                }

                if (existingEntry.Modified >= termEntryModificationDate.ToUniversalTime())
                {
                    continue;
                }

                entryModification = new EntryUpdate
                {
                    Id = existingEntry.Id,
                    ConcurrencyToken = existingEntry.ConcurrencyToken,
                    OfficeId = entryOfficeId,
                    CollectionId = entryCollectionId,
                    Status = status,
                    Reliability = reliability,
                    SubjectIds = subjectIds,
                    DomainIds = domainIds,
                    AdminInfos = adminInfos,
                    Comment = comment,
                    LanguageDetails = new List<EntryModificationLanguageDetail>()
                };
            }

            foreach (var langSet in termEntry.Elements("langSet"))
            {
                var sequence = 0;

                if (!TryGetLanguageId(languages, langSet, out var languageId))
                {
                    var (lineNumber, linePosition) = GetLineInfos(langSet);
                    throw new BusinessValidationException("langSet",
                        $"{ComposeLineInfo(lineNumber, linePosition)} The langSet-language is missing or not supported");
                }

                foreach (var ntigItem in langSet.Descendants("ntig"))
                {
                    if (!Guid.TryParse(ntigItem.Descendants("termNote")
                                .SingleOrDefault(termNote => termNote.Attribute("type")?.Value == "GUID")?.Value,
                            out var ntigGuid))
                    {
                        var (lineNumber, linePosition) = GetLineInfos(ntigItem);
                        throw new BusinessValidationException("ntigItem",
                            $"{ComposeLineInfo(lineNumber, linePosition)} Missing or invalid guid inside ntig element");
                    }

                    sequence++;
                    var languageDetail = new EntryModificationLanguageDetail
                    {
                        ExternalSystemId = ntigGuid.ToString(),
                        LanguageId = (int)languageId,
                        Sequence = sequence
                    };
                    entryModification.LanguageDetails.Add(languageDetail);

                    languageDetail.Context = GetDescriptionValue(ntigItem, "context");

                    var contextSource = GetDescriptionNoteValue(ntigItem, "contextType");
                    languageDetail.ContextSource = AdjustLinkStructure(contextSource);

                    languageDetail.Note = GetTermNoteValue(ntigItem, "usageNote");
                    var noteSource = GetTermNoteValue(ntigItem, "usageNoteSource");
                    languageDetail.NoteSource = AdjustLinkStructure(noteSource);

                    var terminus = ntigItem.Element("termGrp")?.Element("term")?.Value;
                    if (string.IsNullOrWhiteSpace(terminus))
                    {
                        var (lineNumber, linePosition) = GetLineInfos(ntigItem);
                        throw new BusinessValidationException(nameof(terminus),
                            $"{ComposeLineInfo(lineNumber, linePosition)} Term is missing or empty");
                    }

                    var entrySource = ntigItem.Descendants("admin")
                        .SingleOrDefault(admin => admin.Attribute("type")?.Value == "entrySource")?.Value;
                    entrySource = AdjustLinkStructure(entrySource);

                    var termType = GetTermNoteValue(ntigItem, "termType");

                    switch (termType)
                    {
                        case "negative term":
                        {
                            languageDetail.Terminus = terminus;
                            languageDetail.TerminusSource = entrySource;
                            languageDetail.Note = languageId switch
                            {
                                2 => "USG: zu vermeiden",
                                6 => "USG: à éviter",
                                7 => "USG: da evitare",
                                8 => "USG: d'evitar",
                                _ => "USG: to avoid"
                            };
                            break;
                        }
                        case "abbreviation":
                        {
                            languageDetail.Abbreviation = terminus;
                            languageDetail.AbbreviationSource = entrySource;
                            break;
                        }
                        case "fullForm":
                        {
                            languageDetail.Terminus = terminus;
                            languageDetail.TerminusSource = entrySource;
                            languageDetail.Definition = GetDescriptionValue(ntigItem, "definition") ??
                                                        GetDescriptionValue(langSet, "definition");
                            var definitionSource = GetDescriptionNoteValue(ntigItem, "definitionType") ??
                                                   GetDescriptionNoteValue(langSet, "definitionType");
                            languageDetail.DefinitionSource = AdjustLinkStructure(definitionSource);
                            break;
                        }
                        default:
                        {
                            languageDetail.Terminus = terminus;
                            languageDetail.TerminusSource = entrySource;
                            break;
                        }
                    }
                }
            }

            result.Add(entryModification);
        }

        return result;
    }

    private void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        //Note: Warnings are currently disabled because the TBX-Core schema does not match TBX 2.0 data files, so multiple warnings would be generated.
        switch (e.Severity)
        {
            case XmlSeverityType.Error:
                validationErrors.Add(new KeyValuePair<string, string>(e.Severity.ToString(),
                    $"{ComposeLineInfo(e.Exception.LineNumber, e.Exception.LinePosition)} {e.Message}"));
                break;
            case XmlSeverityType.Warning:
                break;
            default:
                break;
        }
    }

    private static string GetTermNoteValue(XElement ntigElement, string termNoteType)
    {
        return ntigElement.Descendants("termNote")
            .SingleOrDefault(termNote => termNote.Attribute("type")?.Value == termNoteType)?.Value;
    }

    private static string GetDescriptionValue(XElement ntigElement, string descriptionType)
    {
        return ntigElement.Elements("descripGrp").Elements("descrip")
            .SingleOrDefault(description => description.Attribute("type")?.Value == descriptionType)?.Value;
    }

    private static string GetDescriptionNoteValue(XElement ntigElement, string descriptionNoteType)
    {
        return ntigElement.Elements("descripGrp").Elements("descripNote")
            .SingleOrDefault(descriptionNote => descriptionNote.Attribute("type")?.Value == descriptionNoteType)?.Value;
    }

    private static bool TryGetLanguageId(IList<LanguageItem> availableLanguages, XElement languageSet, out int? result)
    {
        result = default;

        var langAttributeValue = languageSet.Attributes().SingleOrDefault(a => a.Name.LocalName == "lang")?.Value;

        if (string.IsNullOrWhiteSpace(langAttributeValue))
        {
            return false;
        }

        var languageId = availableLanguages.SingleOrDefault(languageItem =>
            langAttributeValue.StartsWith($"{languageItem.TwoLetterIsoCode}-",
                StringComparison.InvariantCultureIgnoreCase))?.LanguageId;

        if (!languageId.HasValue)
        {
            return false;
        }

        result = languageId;
        return true;
    }

    protected static string AdjustLinkStructure(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return source;
        }

        try
        {
            //Note: These link structure parsings are based on the test data files and their data structures.
            //      It may need to be adapted and extended for other data files containing different data structures.
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
            {
                // Plain link content
                source = $"[{uri}]({uri})";
            }
            else if (source.StartsWith(@"\h!http", StringComparison.InvariantCultureIgnoreCase))
            {
                // Structured link content
                var segments = source.Split('!');
                var link = segments[1];
                var linkTextStringBuilder = new StringBuilder(segments[2].Trim());

                var linkText = linkTextStringBuilder
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(@" \s", ", ")
                    .Replace(@"\s ", ", ")
                    .ToString();

                source = $"([{linkText}]({link}))";
            }
            else if (source.Contains(@"\h!", StringComparison.InvariantCultureIgnoreCase))
            {
                // Text and multiple links content - V1
                var entrySourceStringBuilder = new StringBuilder();

                var segments = source.Split(@"\h!");
                foreach (var segment in segments)
                {
                    if (segment.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var rawLinkSegments = segment.Split(@"\s");
                        var linkSegments = rawLinkSegments[0].Split('!');
                        entrySourceStringBuilder.Append(CultureInfo.InvariantCulture, $"[{linkSegments[1]}]({linkSegments[0]})");
                        entrySourceStringBuilder.Append(rawLinkSegments[1]);
                    }
                    else
                    {
                        entrySourceStringBuilder.Append(segment);
                    }
                }

                source = entrySourceStringBuilder.ToString();
            }
            else if (source.Contains(@"; \h", StringComparison.InvariantCultureIgnoreCase))
            {
                // Text and multiple links content - V2
                var entrySourceStringBuilder = new StringBuilder();

                var segments = source.Split(@"; \h");
                foreach (var segment in segments)
                {
                    if (segment.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var rawLinkSegments = segment.Split(@"\s");
                        entrySourceStringBuilder.Append(CultureInfo.InvariantCulture, $" [{rawLinkSegments[0]}]({rawLinkSegments[0]})");
                        entrySourceStringBuilder.Append(rawLinkSegments[1]);
                    }
                    else
                    {
                        entrySourceStringBuilder.Append(segment);
                    }
                }

                source = entrySourceStringBuilder.ToString();
            }

            return source;
        }
        catch
        {
            // Note: In the event of a content parsing error, the original content is simply returned.
            return source;
        }
    }

    private static async Task<MemoryStream> ReadAsSanitizedXmlMemoryStreamAsync(IFormFile file)
    {
        // Note:
        // To get rid of invalid xml chars (e.g. STX), we need to sanitize it and return it as a stream for processing.
        // The disadvantage of this is that we have to read the content twice.
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        return new MemoryStream(Encoding.UTF8.GetBytes(content.AsSanitizedXmlString() ?? string.Empty));
    }

    private async Task<IList<LanguageItem>> GetLanguages()
    {
        var languages = await languageRepository.GetAll();
        return languages.Select(l => new LanguageItem { LanguageId = l.Id, TwoLetterIsoCode = l.IsoCode }).ToList();
    }

    private static (int lineNumber, int linePosition) GetLineInfos(XElement xElement)
    {
        var lineNumber = -1;
        var linePosition = -1;

        if (xElement is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
        {
            lineNumber = xmlLineInfo.LineNumber;
            linePosition = xmlLineInfo.LinePosition;
        }

        return (lineNumber, linePosition);
    }

    private static string ComposeLineInfo(int lineNumber, int linePosition)
    {
        return $"Line number {lineNumber}, position {linePosition}:";
    }

    private sealed record LanguageItem
    {
        public int LanguageId { get; init; }
        public string TwoLetterIsoCode { get; init; }
    }
}
