using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Swiss.FCh.Utils.Models;
using Swiss.FCh.Utils.Services;
using Reliability = Bk.Termdat.Business.Resource.Models.Reliability;
using Status = Bk.Termdat.Business.Resource.Models.Status;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class TbxDataServiceTests
{
    private readonly IOptions<FileImportOptions> optionsMock = Substitute.For<IOptions<FileImportOptions>>();
    private readonly IOptions<EmailOptions> emailOptionsMock = Substitute.For<IOptions<EmailOptions>>();

    [Test]
    public void AdjustLinkStructure_WithStructuredLinkContentOnly_ShouldAdjustToLinkStructure()
    {
        var source =
            @"\h!https://www.fedlex.admin.ch/eli/cc/2000/282/it! Ordinanza sull’organizzazione del Dipartimento federale dell’interno; RS 172.212.1\s (30.03.2021)";

        var result = TestableTbxDataService.AdjustLinkStructureAccessor(source);

        Assert.That(result,
            Is.EqualTo(
                "([Ordinanza sull’organizzazione del Dipartimento federale dell’interno; RS 172.212.1, 30.03.2021](https://www.fedlex.admin.ch/eli/cc/2000/282/it))"));
    }

    [Test]
    public void AdjustLinkStructure_WithPlainLinkOnly_ShouldAdjustToLinkStructure()
    {
        var source = @"https://www.eawag.ch/fr/portrait/portrait/organisation/";

        var result = TestableTbxDataService.AdjustLinkStructureAccessor(source);

        Assert.That(result,
            Is.EqualTo(
                "[https://www.eawag.ch/fr/portrait/portrait/organisation/](https://www.eawag.ch/fr/portrait/portrait/organisation/)"));
    }

    [Test]
    public void AdjustLinkStructure_WithNotParsableContent_ShouldReturnRawContent()
    {
        var source = @"https://www.eawag.ch;";

        var result = TestableTbxDataService.AdjustLinkStructureAccessor(source);

        Assert.That(result, Is.EqualTo("https://www.eawag.ch;"));
    }

    [Test]
    public void AdjustLinkStructure_WithComplexContentV1_ShouldAdjustAllLinksToLinkStructure()
    {
        var source = """
                     ...causes it, 2020
                                     (\h!https://www.who.int/emergencies/diseases/novel-coronavirus-2019/technical-guidance/naming-the-coronavirus-disease-covid-2019!Internet,
                                     2020-03-11\s)); (c) nach BAG, Krankheiten &gt; Infektionskrankheiten: Ausbrüche,
                                     Epidemien, Pandemien &gt; Aktuelle Ausbrüche und Epidemien &gt; Neues Coronavirus
                                     (\h!https://www.bag.admin.ch/bag/de/home/krankheiten/ausbrueche-epidemien-pandemien/aktuelle-ausbrueche-epidemien/novel-cov.html!Internet,
                                     2020-03-03\s), Helmholtz...
                     """;

        var actual = TestableTbxDataService.AdjustLinkStructureAccessor(source);

        Assert.That(actual, Is.EqualTo("""
                                       ...causes it, 2020
                                                       ([Internet,
                                                       2020-03-11](https://www.who.int/emergencies/diseases/novel-coronavirus-2019/technical-guidance/naming-the-coronavirus-disease-covid-2019))); (c) nach BAG, Krankheiten &gt; Infektionskrankheiten: Ausbrüche,
                                                       Epidemien, Pandemien &gt; Aktuelle Ausbrüche und Epidemien &gt; Neues Coronavirus
                                                       ([Internet,
                                                       2020-03-03](https://www.bag.admin.ch/bag/de/home/krankheiten/ausbrueche-epidemien-pandemien/aktuelle-ausbrueche-epidemien/novel-cov.html)), Helmholtz...
                                       """));
    }

    [Test]
    public void AdjustLinkStructure_WithComplexContentV2_ShouldAdjustAllLinksToLinkStructure()
    {
        var source =
            @"Internet DFAE, DSC, Partenariati in materia di migrazione; \hhttps://www.eda.admin.ch/deza/it/home/temi-dsc/migrazione/dialogo-migrazioni/migrationspartnerschaften.html\s (23.8.2024)";

        var result = TestableTbxDataService.AdjustLinkStructureAccessor(source);

        Assert.That(result,
            Is.EqualTo(
                "Internet DFAE, DSC, Partenariati in materia di migrazione [https://www.eda.admin.ch/deza/it/home/temi-dsc/migrazione/dialogo-migrazioni/migrationspartnerschaften.html](https://www.eda.admin.ch/deza/it/home/temi-dsc/migrazione/dialogo-migrazioni/migrationspartnerschaften.html) (23.8.2024)"));
    }

    [Test]
    public async Task ImportDocument_WitValidData_ShouldImportDataSuccessful()
    {
        var currentUserEmail = "a@b.c";
        var importJobLabel = "Tbx-Import-666";
        var languageRepositoryMock = Substitute.For<ILanguageRepository>();
        var entryRepositoryMock = Substitute.For<IEntryRepository>();
        var entryServiceMock = Substitute.For<IEntryService>();
        var emailServiceMock = Substitute.For<IEmailService>();
        Email emailToSend = null;
        emailServiceMock.Send(Arg.Do<Email>(v => emailToSend = v)).Returns(Task.CompletedTask);
        var emailOptions = new EmailOptions { SenderAddress = "test_sender_email" };
        emailOptionsMock.Value.Returns(emailOptions);
        optionsMock.Value.Returns(new FileImportOptions { Email = "configured-processing-user" });
        languageRepositoryMock.GetAll()
            .Returns(new List<Language>
            {
                new() { Id = 2, IsoCode = "DE" },
                new() { Id = 3, IsoCode = "EN" },
                new() { Id = 6, IsoCode = "FR" },
                new() { Id = 7, IsoCode = "IT" },
                new() { Id = 8, IsoCode = "RM" }
            });
        // Note: This entry should be skipped
        entryRepositoryMock.GetByExternalSystemId("00cb09aa-ffc2-4008-833a-bba336aec164").Returns(new Entry
        {
            ExternalSystemId = "00cb09aa-ffc2-4008-833a-bba336aec164",
            Modified = DateTime.Parse("2024-02-15T10:09:10Z", CultureInfo.InvariantCulture).ToUniversalTime()
        });
        // Note: This entry should be updated because it is one second older than the entry in the file
        entryRepositoryMock.GetByExternalSystemId("aa44b3eb-07d6-47ca-a9e4-d44c2cb5aaf9").Returns(new Entry
        {
            ExternalSystemId = "aa44b3eb-07d6-47ca-a9e4-d44c2cb5aaf9",
            Modified = DateTime.Parse("2024-02-19T09:14:20Z", CultureInfo.InvariantCulture).ToUniversalTime().AddSeconds(-1),
            ConcurrencyToken = 666,
            Id = 42
        });
        using var testDataReader = new StreamReader(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Bk.Termdat.Business.Tests.Assets.Tbx.Testdata.tbx"));
        var xDocument = XDocument.Load(testDataReader, LoadOptions.SetLineInfo);
        var sut = new TbxDataService(optionsMock, new NullLogger<TbxDataService>(), languageRepositoryMock,
            entryRepositoryMock, entryServiceMock, emailServiceMock, emailOptionsMock);

        await sut.ImportDocument(xDocument, importJobLabel, currentUserEmail, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int> { 3, 4 }, new List<int> { 5, 6 }, new List<int> { 7, 8 },
            new List<AdminInfoSummary> { new AdminInfoDetail { Code = "A" } }, "B");

        await entryServiceMock.Received(3).Create(Arg.Any<EntryCreate>(), "configured-processing-user");
        await entryServiceMock.Received(1).Update(Arg.Any<EntryUpdate>(), "configured-processing-user");
        await emailServiceMock.Received(1).Send(Arg.Is(emailToSend));

        Assert.Multiple(() =>
        {
            Assert.That(emailToSend.From.Name, Is.EqualTo(emailOptions.SenderAddress));
            Assert.That(emailToSend.From.Address, Is.EqualTo(emailOptions.SenderAddress));
            Assert.That(emailToSend.To.First().Address, Is.EqualTo(currentUserEmail));
            Assert.That(emailToSend.To.First().Address, Is.EqualTo(currentUserEmail));
            Assert.That(emailToSend.Subject, Is.EqualTo(importJobLabel));
        });
    }

    [Test]
    public async Task ComposeEntryModificationItems_WithValidData_ShouldMapToCreateAndUpdateEntries()
    {
        var languageRepositoryMock = Substitute.For<ILanguageRepository>();
        var entryRepositoryMock = Substitute.For<IEntryRepository>();
        languageRepositoryMock.GetAll()
            .Returns(new List<Language>
            {
                new() { Id = 2, IsoCode = "DE" },
                new() { Id = 3, IsoCode = "EN" },
                new() { Id = 6, IsoCode = "FR" },
                new() { Id = 7, IsoCode = "IT" },
                new() { Id = 8, IsoCode = "RM" }
            });

        // Note: This entry should be skipped
        entryRepositoryMock.GetByExternalSystemId("00cb09aa-ffc2-4008-833a-bba336aec164").Returns(new Entry
        {
            ExternalSystemId = "00cb09aa-ffc2-4008-833a-bba336aec164",
            Modified = DateTime.Parse("2024-02-15T10:09:10Z", CultureInfo.InvariantCulture).ToUniversalTime()
        });

        // Note: This entry should be updated because it is one second older than the entry in the file
        entryRepositoryMock.GetByExternalSystemId("aa44b3eb-07d6-47ca-a9e4-d44c2cb5aaf9").Returns(new Entry
        {
            ExternalSystemId = "aa44b3eb-07d6-47ca-a9e4-d44c2cb5aaf9",
            Modified = DateTime.Parse("2024-02-19T09:14:20Z", CultureInfo.InvariantCulture).ToUniversalTime().AddSeconds(-1),
            ConcurrencyToken = 666,
            Id = 42
        });

        using var testDataReader = new StreamReader(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Bk.Termdat.Business.Tests.Assets.Tbx.Testdata.tbx"));
        var xDocument = XDocument.Load(testDataReader, LoadOptions.SetLineInfo);
        var sut = new TestableTbxDataService(optionsMock, languageRepositoryMock, entryRepositoryMock, null, null,
            emailOptionsMock);

        var actual = await sut.ComposeEntryModificationItemsAccessor(xDocument, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int> { 3, 4 }, new List<int> { 5, 6 }, new List<int> { 7, 8 },
            new List<AdminInfoSummary> { new AdminInfoDetail { Code = "A" } }, "B");

        // Note: The import-file actually contains 5 elements
        Assert.That(actual.Count, Is.EqualTo(4));
        var entryCreateItem = actual.Single(item => item is EntryCreate
        {
            ExternalSystemId: "e0fe45f6-61c9-40d3-b965-78b016151550"
        });
        Assert.That(entryCreateItem, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(entryCreateItem.Comment, Is.EqualTo("B"));
            Assert.That(entryCreateItem.OfficeId, Is.EqualTo(1));
            Assert.That(entryCreateItem.CollectionId, Is.EqualTo(2));
            Assert.That(entryCreateItem.SubjectIds, Is.EquivalentTo(new List<int> { 3, 4 }));
            Assert.That(entryCreateItem.DomainIds, Is.EquivalentTo(new List<int> { 5, 6 }));
            Assert.That(entryCreateItem.AdminInfos.First().Code, Is.EqualTo("A"));
        });
        var firstLanguageDetailOfFirstItem = entryCreateItem.LanguageDetails.First();
        Assert.Multiple(() =>
        {
            Assert.That(firstLanguageDetailOfFirstItem.LanguageId, Is.EqualTo(2));
            Assert.That(firstLanguageDetailOfFirstItem.ExternalSystemId,
                Is.EqualTo("603e7387-000a-4849-82ad-0e8a1b5428b2"));
            Assert.That(firstLanguageDetailOfFirstItem.Context, Is.EqualTo("Kontext Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.ContextSource, Is.EqualTo("Quelle Kontext Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.Note, Is.EqualTo("Anmerkung Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.NoteSource, Is.EqualTo("Quelle Anmerkung Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.Terminus, Is.EqualTo("Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.TerminusSource, Is.EqualTo("Quelle Terminus Benennung"));
            Assert.That(firstLanguageDetailOfFirstItem.Definition, Is.EqualTo("Definition (Ebene Terminus Benennung)"));
            Assert.That(firstLanguageDetailOfFirstItem.DefinitionSource,
                Is.EqualTo("Quelle Definition (Ebene Terminus Benennung)"));
        });
        var secondLanguageDetailOfFirstItem = entryCreateItem.LanguageDetails[1];
        Assert.Multiple(() =>
        {
            Assert.That(secondLanguageDetailOfFirstItem.Abbreviation, Is.EqualTo("Abkürzung"));
            Assert.That(secondLanguageDetailOfFirstItem.AbbreviationSource, Is.EqualTo("Quelle Abkürzung"));
            Assert.That(secondLanguageDetailOfFirstItem.Terminus, Is.Null);
            Assert.That(secondLanguageDetailOfFirstItem.TerminusSource, Is.Null);
        });
        var thirdLanguageDetailOfFirstItem = entryCreateItem.LanguageDetails[2];
        Assert.Multiple(() =>
        {
            Assert.That(thirdLanguageDetailOfFirstItem.Terminus, Is.EqualTo("Terminus Synonym"));
            Assert.That(thirdLanguageDetailOfFirstItem.TerminusSource, Is.EqualTo("Quelle Terminus Synonym"));
        });
        var fourthLanguageDetailOfFirstItem = entryCreateItem.LanguageDetails[3];
        Assert.Multiple(() =>
        {
            Assert.That(fourthLanguageDetailOfFirstItem.Terminus, Is.EqualTo("Terminus + USG: zu vermeiden"));
            Assert.That(fourthLanguageDetailOfFirstItem.TerminusSource, Is.EqualTo("Quelle Terminus (zu vermeiden)"));
            Assert.That(fourthLanguageDetailOfFirstItem.Note, Is.EqualTo("USG: zu vermeiden"));
        });
        var entryUpdateItem = actual.Single(item => item is EntryUpdate) as EntryUpdate;
        Assert.That(entryUpdateItem, Is.Not.Null);
        Assert.That(entryUpdateItem.Id, Is.EqualTo(42));
        Assert.That(entryUpdateItem.ConcurrencyToken, Is.EqualTo(666));
    }
}
