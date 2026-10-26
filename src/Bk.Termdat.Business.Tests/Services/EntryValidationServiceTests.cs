using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using DomainModel = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
[SetUICulture("de")]
internal class EntryValidationServiceTests
{
    private EntryValidationService service;

    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private readonly ICollectionRepository collectionRepositoryMock = Substitute.For<ICollectionRepository>();
    private readonly IOfficeRepository officeRepositoryMock = Substitute.For<IOfficeRepository>();
    private readonly ILenochClassificationRepository lenochClassificationRepositoryMock = Substitute.For<ILenochClassificationRepository>();
    private readonly IConceptSystemPositionRepository conceptSystemPositionRepositoryMock = Substitute.For<IConceptSystemPositionRepository>();
    private readonly ILanguageRepository languageRepositoryMock = Substitute.For<ILanguageRepository>();
    private readonly IEntryNotePatternService entryNotePatternServiceMock = Substitute.For<IEntryNotePatternService>();
    private readonly IGlossaryRepository glossaryRepositoryMock = Substitute.For<IGlossaryRepository>();
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly ISubjectRepository subjectRepositoryMock = Substitute.For<ISubjectRepository>();
    private readonly IDomainRepository domainRepositoryMock = Substitute.For<IDomainRepository>();
    private readonly IAdminInfoRepository adminInfoRepositoryMock = Substitute.For<IAdminInfoRepository>();

    private DomainModel.Entry entry;
    private DomainModel.Collection collection;
    private DomainModel.Office office;
    private DomainModel.ConceptSystemPosition conceptSystemPosition;
    private EntryCreate entryModification;
    private IList<DomainModel.Language> languages;
    private DomainModel.Picture picture;
    private List<DomainModel.Domain> domains;
    private List<DomainModel.Subject> subjects;
    private List<DomainModel.AdminInfo> adminInfos;
    private List<AdminInfoSummary> adminInfoSummaries;

    [SetUp]
    public void SetUp()
    {
        authorizationServiceMock.ClearSubstitute();
        collectionRepositoryMock.ClearSubstitute();
        officeRepositoryMock.ClearSubstitute();
        lenochClassificationRepositoryMock.ClearSubstitute();
        conceptSystemPositionRepositoryMock.ClearSubstitute();
        languageRepositoryMock.ClearSubstitute();
        entryNotePatternServiceMock.ClearSubstitute();
        glossaryRepositoryMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        subjectRepositoryMock.ClearSubstitute();
        domainRepositoryMock.ClearSubstitute();
        adminInfoRepositoryMock.ClearSubstitute();

        adminInfoSummaries = new List<AdminInfoSummary>
        {
            new() { Id = 33, Code = "Test1" },
            new() { Id = 34, Code = "Test2" },
        };

        entry = new DomainModel.Entry
        {
            Id = 1,
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            Translations = new List<DomainModel.EntryLanguage>(),
            Collection = new()
            {
                Translations = new List<DomainModel.CollectionTranslation>()
            },
            Office = new(),
            ConceptSystemPosition = new(),
            References = new List<DomainModel.EntryReference>()
            {
                new() { ReferencedEntryId = 2, Type = DomainModel.ReferenceType.Antonym },
                new() { ReferencedEntryId = 3, Type = DomainModel.ReferenceType.None },
            }
        };
        entryModification = new EntryCreate
        {
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            LanguageDetails = new List<EntryModificationLanguageDetail> { new() { LanguageId = 1, Sequence = 1 } },
            References = new List<EntryModificationReference>
            {
                new() { EntryId = 2, Type = ReferenceType.Antonym },
                new() { EntryId = 3, Type = ReferenceType.None },
            },
            DomainIds = new List<int> { 1, 2 },
            SubjectIds = new List<int> { 1, 2 },
            AdminInfos = adminInfoSummaries,
        };
        picture = new()
        {
            EntryId = 1,
            Translations = new List<DomainModel.PictureTranslation>()
            {
                new() { EntryId = 1, LanguageId = 1, Description = "Description DE" },
                new() { EntryId = 1, LanguageId = 2, Description = "Description ES" },
            }
        };
        var foundEntries = new List<DomainModel.Entry>
        {
            new() { Id = 2 },
            new() { Id = 3 },
        };
        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(foundEntries);

        collection = new() { Id = 1 };
        collectionRepositoryMock.GetById(1).Returns(collection);

        office = new() { Id = 1 };
        officeRepositoryMock.GetById(1).Returns(office);

        conceptSystemPosition = new() { Id = 1, Value = "1.1 Wichtiges" };
        conceptSystemPositionRepositoryMock.GetById(1).Returns(conceptSystemPosition);

        languages = new List<DomainModel.Language>()
        {
            new() { Id = 1, IsoCode = "DE" },
            new() { Id = 2, IsoCode = "ES" },
            new() { Id = 3, IsoCode = "FR" },
        };
        languageRepositoryMock.GetAll().Returns(languages);

        subjects = new List<DomainModel.Subject>
        {
            new() { Id = 1 },
            new() { Id = 2 },
        };
        subjectRepositoryMock.GetByIds(Arg.Any<IList<int>>()).Returns(subjects);
        domains = new List<DomainModel.Domain>
        {
            new() { Id = 1, SubjectId = 1 },
            new() { Id = 2, SubjectId = 2 },
        };
        domainRepositoryMock.GetByIds(Arg.Any<IList<int>>()).Returns(domains);

        adminInfos = new List<DomainModel.AdminInfo>
        {
            new() { Id = 1, Code = "Info1" },
            new() { Id = 2, Code = "Info2" },
        };

        adminInfoRepositoryMock.GetByCodes(Arg.Any<IList<string>>()).Returns(adminInfos);

        service = new EntryValidationService(
            authorizationServiceMock,
            collectionRepositoryMock,
            officeRepositoryMock,
            conceptSystemPositionRepositoryMock,
            lenochClassificationRepositoryMock,
            languageRepositoryMock,
            adminInfoRepositoryMock,
            entryRepositoryMock,
            entryNotePatternServiceMock,
            glossaryRepositoryMock,
            subjectRepositoryMock,
            domainRepositoryMock
        );
    }

    [TearDown]
    public void TearDown()
    {
        collectionRepositoryMock.ClearReceivedCalls();
        officeRepositoryMock.ClearReceivedCalls();
        lenochClassificationRepositoryMock.ClearReceivedCalls();
        conceptSystemPositionRepositoryMock.ClearReceivedCalls();
        entryNotePatternServiceMock.ClearSubstitute();
    }

    [TestCase("First broken link without outer brackets [text](http://test.ch&1=e((1)&b=3) Second Link [text](http://test.ch&1=e(4)&b=5)",
        "First broken link without outer brackets [text](http://test.ch&1=e((1)&b=3) Second Link [text](http://test.ch&1=e%284%29&b=5)")]
    [TestCase("First broken link ([text](http://test.ch&1=e((1)&b=3) Second Link [text](http://test.ch&1=e(4)&b=5)",
        "First broken link ([text](http://test.ch&1=e((1)&b=3) Second Link [text](http://test.ch&1=e%284%29&b=5)")]
    [TestCase("Link in migrated format ([Linktext, Datum](www.test.ch/test/))", "Link in migrated format ([Linktext, Datum](www.test.ch/test/))")]
    [TestCase("Link with brackets ([Linktext, Datum][Linktext, Datum](https://eur-lex.europa.eu/legal-content/DE/TXT/PDF/?uri=CELEX:32018H0604(01)&from=SV))",
        "Link with brackets ([Linktext, Datum][Linktext, Datum](https://eur-lex.europa.eu/legal-content/DE/TXT/PDF/?uri=CELEX:32018H0604%2801%29&from=SV))")]
    [TestCase("First link ([text](http://test.ch&1=e(1)&b=3)) Second Link ([text](http://test.ch&1=e(4)&b=5))",
        "First link ([text](http://test.ch&1=e%281%29&b=3)) Second Link ([text](http://test.ch&1=e%284%29&b=5))")]
    [TestCase("no link here", "no link here")]
    [TestCase("link alone is ignored: www.test.ch", "link alone is ignored: www.test.ch")]
    [TestCase("Link without outer round brackets [Linktext, Datum](www.test.ch/test/)", "Link without outer round brackets [Linktext, Datum](www.test.ch/test/)")]
    [TestCase("Just link without text, Datum](www.test.ch/test/)", "Just link without text, Datum](www.test.ch/test/)")]
    [TestCase("[{}]", "[{}]")]
    [TestCase("", null)]
    public void EscapeBracketsInHyperlinksInSources_ShouldEscapeBrackets_WhenInnerBrackets(string input, string expected)
    {
        var detail = new EntryModificationLanguageDetail { AbbreviationSource = input };

        service.EscapeBracketsInHyperlinksInSources(detail);

        Assert.That(detail.AbbreviationSource, Is.EqualTo(expected));
    }

    [TestCase(new[] { 1, 3, 4 })]
    [TestCase(new[] { 0, 1, 2, 3 })]
    [TestCase(new[] { 1, 3, 4 })]
    public void ValidateLanguageDetailsThrowsOnInvalidSequence(int[] sequence)
    {
        var languageList = Enumerable.Range(1, 4);
        entryModification.LanguageDetails = languageList
                .Select(l => sequence.Select(s => new EntryModificationLanguageDetail { LanguageId = l, Sequence = s }))
                .SelectMany(l => l)
                .ToList();

        Assert.That(() => service.ValidateLanguageDetails(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public void ValidateLanguageDetailsValidatesSequence()
    {
        var languageList = Enumerable.Range(1, 4);
        var sequence = Enumerable.Range(1, 6);
        entryModification.LanguageDetails = languageList
                .Select(l => sequence.Select(s => new EntryModificationLanguageDetail { LanguageId = l, Sequence = s }))
                .SelectMany(l => l)
                .ToList();

        service.ValidateLanguageDetails(entryModification);
        entryNotePatternServiceMock.Received(entryModification.LanguageDetails.Count).DetermineEntryNotePatterns(Arg.Any<string>());
    }

    [Test]
    public async Task ValidateCollectionReturnsCollection()
    {
        var actual = await service.ValidateCollection(entryModification);

        Assert.That(actual, Is.EqualTo(collection));
    }

    [Test]
    public void ValidateCollectionThrowsOnNotExistingCollection()
    {
        collectionRepositoryMock.GetById(1).ReturnsNull();
        Assert.That(() => service.ValidateCollection(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateOfficeReturnsOffice()
    {
        var actual = await service.ValidateOffice(entryModification);

        Assert.That(actual, Is.EqualTo(office));
    }

    [Test]
    public void ValidateOfficeThrowsOnNotExistingOffice()
    {
        officeRepositoryMock.GetById(1).ReturnsNull();
        Assert.That(() => service.ValidateOffice(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateConceptSystemReturnsConceptSystem()
    {
        var actual = await service.ValidateConceptSystemPosition(entryModification);
        Assert.That(actual, Is.EqualTo(conceptSystemPosition));
    }

    [Test]
    public void ValidateConceptSystemThrowsOnNotExistingConceptSystem()
    {
        conceptSystemPositionRepositoryMock.GetById(1).ReturnsNull();
        Assert.That(() => service.ValidateConceptSystemPosition(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public void ValidateConceptSystemPositionThrowsOnNotMatchingConceptSystemPosition()
    {
        entryModification.ConceptSystemPositionId = 3;
        conceptSystemPositionRepositoryMock.GetById(1).Returns(new DomainModel.ConceptSystemPosition { Id = 2, Value = "1.2 Unwichtiges" });
        Assert.That(() => service.ValidateConceptSystemPosition(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public void ValidateRelatedCollectionsThrowsOnInvalidRelatedCollections()
    {
        var collectionIds = Enumerable.Range(1, 3).ToList();
        entryModification.RelatedCollectionIds = collectionIds;
        var relatedCollections = collectionIds.Select(id => new DomainModel.Collection { Id = id + 5 }).ToList();
        entry.RelatedCollections = relatedCollections;

        collectionRepositoryMock.GetByIds(collectionIds).Returns(relatedCollections);

        Assert.That(() => service.ValidateRelatedCollections(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateRelatedCollectionsLoadsRelatedCollections()
    {
        var collectionIds = Enumerable.Range(1, 3).ToList();
        entryModification.RelatedCollectionIds = collectionIds;
        var relatedCollections = collectionIds.Select(id => new DomainModel.Collection { Id = id }).ToList();
        entry.RelatedCollections = relatedCollections;

        collectionRepositoryMock.GetByIds(collectionIds).Returns(relatedCollections);

        var actual = await service.ValidateRelatedCollections(entryModification);

        Assert.That(actual, Is.EqualTo(relatedCollections));
        await collectionRepositoryMock.Received()
            .GetByIds(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(collectionIds)));
    }

    [Test]
    public async Task ValidateRelatedCollectionsReturnsEmpty()
    {
        entryModification.RelatedCollectionIds = null;

        var result = await service.ValidateRelatedCollections(entryModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateLenochClassificationsThrowsOnInvalidLenochClassifications()
    {
        var lenochClassificationIds = Enumerable.Range(3, 5).ToList();
        entryModification.LenochClassificationIds = lenochClassificationIds;
        var lenochClassifications = lenochClassificationIds.Select(id => new DomainModel.LenochClassification { Id = id + 10 }).ToList();
        entry.LenochClassifications = lenochClassifications;
        lenochClassificationRepositoryMock.GetByIds(lenochClassificationIds).Returns(lenochClassifications);

        Assert.That(() => service.ValidateLenochClassification(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateLenochClassificationsLoadsLenochClassifications()
    {
        var lenochClassificationIds = Enumerable.Range(3, 5).ToList();
        entryModification.LenochClassificationIds = lenochClassificationIds;
        var lenochClassifications = lenochClassificationIds.Select(id => new DomainModel.LenochClassification { Id = id }).ToList();
        entry.LenochClassifications = lenochClassifications;

        lenochClassificationRepositoryMock.GetByIds(lenochClassificationIds).Returns(lenochClassifications);

        var actual = await service.ValidateLenochClassification(entryModification);

        Assert.That(actual, Is.EqualTo(lenochClassifications));
        await lenochClassificationRepositoryMock.Received()
            .GetByIds(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(lenochClassificationIds)));
    }

    [Test]
    public async Task ValidateLenochClassificationsReturnsEmpty()
    {
        entryModification.LenochClassificationIds = null;

        var result = await service.ValidateLenochClassification(entryModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateGlossariesThrowsOnInvalidGlossaries()
    {
        var glossaryIds = Enumerable.Range(3, 5).ToList();
        entryModification.GlossaryIds = glossaryIds;
        var glossaries = glossaryIds.Select(id => new DomainModel.Glossary { Id = id + 10 }).ToList();
        entry.Glossaries = glossaries;
        glossaryRepositoryMock.GetByIds(glossaryIds).Returns(glossaries);

        Assert.That(() => service.ValidateGlossaries(entryModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateGlossariesLoadsGlossaries()
    {
        var glossaryIds = Enumerable.Range(3, 5).ToList();
        entryModification.GlossaryIds = glossaryIds;
        var glossaries = glossaryIds.Select(id => new DomainModel.Glossary { Id = id }).ToList();
        entry.Glossaries = glossaries;

        glossaryRepositoryMock.GetByIds(glossaryIds).Returns(glossaries);

        var actual = await service.ValidateGlossaries(entryModification);

        Assert.That(actual, Is.EqualTo(glossaries));
        await glossaryRepositoryMock.Received()
            .GetByIds(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(glossaryIds)));
    }

    [Test]
    public async Task ValidateGlossariesReturnsEmpty()
    {
        entryModification.GlossaryIds = null;

        var result = await service.ValidateGlossaries(entryModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidatePictureFileThrowsExceptionForEmptyPicture()
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.Length.ReturnsForAnyArgs(100000);

        var exception = Assert.Throws<BusinessValidationException>(() => service.ValidatePictureFile(null, formFile));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ModelKey, Is.EqualTo("picture"));
        Assert.That(exception.Message, Is.EqualTo("Es muss mindestens eine Bildbeschreibung vorhanden sein."));
    }

    [Test]
    [SetCulture("de-CH")]
    public void ValidatePictureFileThrowsExceptionForPicturesTooBig()
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.Length.ReturnsForAnyArgs((int)(2.1 * 1024 * 1024));
        var pictureDto = new PictureDetail();

        var exception = Assert.Throws<BusinessValidationException>(() => service.ValidatePictureFile(pictureDto, formFile));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ModelKey, Is.EqualTo("pictureFile"));
        Assert.That(exception.Message, Is.EqualTo("Die Abbildung überschreitet die maximale Dateigrösse von 2048.00 KB."));
    }

    [Test]
    public void ValidatePictureFileDoesNotThrowExceptionWhenPictureFileIsNull()
    {
        var pictureDto = new PictureDetail();
        service.ValidatePictureFile(pictureDto, null);

        Assert.Pass();
    }

    [Test]
    public async Task CanValidatePictureWithNull()
    {
        var result = await service.ValidatePicture(null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CanValidatePicture()
    {
        var result = await service.ValidatePicture(picture);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Translations.Count, Is.EqualTo(picture.Translations.Count));
        Assert.That(result.Translations.All(t => t.LanguageId == t.Language.Id));
    }

    [Test]
    public void CanValidatePictureThrowsBusinessValidationException()
    {
        picture.Translations.Add(new() { EntryId = 1, Description = "SHOULD THROW ERROR", LanguageId = 10 });

        var exception = Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidatePicture(picture));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.ModelKey, Is.EqualTo("Language"));
        Assert.That(exception.Message, Is.EqualTo("Die Sprache mit der ID='10' wurde nicht gefunden."));
    }

    [Test]
    public async Task ShouldValidateReferencesWhenNewReferencesAreCreated()
    {
        var result = await service.ValidateReferences(entryModification, entry);

        await entryRepositoryMock.Received(1).GetByIds(Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2, 3 })));

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(entry.References));
        Assert.That(result.All(r => r.EntryId == entry.Id));
    }

    [Test]
    public async Task ShouldValidateReferencesWhenNewReferencesAreCreatedAndUpdated()
    {
        entryModification.References[1].Id = 1;
        entry.References.ToList()[1].Id = 1;

        var result = await service.ValidateReferences(entryModification, entry);

        await entryRepositoryMock.Received(1).GetByIds(Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2, 3 })));

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(entry.References));
        Assert.That(result.All(r => r.EntryId == entry.Id));
    }

    [Test]
    public async Task ShouldValidateReferencesForEmptyReferences()
    {
        entryModification.References = null;
        var result = await service.ValidateReferences(entryModification, entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        entryModification.References = new List<EntryModificationReference>();
        var result2 = await service.ValidateReferences(entryModification, entry);

        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.Count, Is.EqualTo(0));
    }

    [Test]
    public void ValidateReferencesThrowsExceptionWhenReferencedEntriesDontExist()
    {
        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>());

        Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidateReferences(entryModification, entry));
    }

    [Test]
    public void ValidateExternalLinks_IfDataIsValid_SetsEntryId()
    {
        var update = new DomainModel.Entry
        {
            Id = 99,
            ExternalLinks = [new DomainModel.EntryExternalLink { Id = 1, Type = DomainModel.ExternalLinkType.Fedlex, Url = "https://example.com" }]
        };

        var result = service.ValidateExternalLinks(new EntryUpdate { ExternalLinks = [new EntryModificationExternalLink()] }, update);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().EntryId, Is.EqualTo(99));
    }

    [Test]
    public void ValidateExternalLinks_IfDataIsInvalid_ThrowsBusinessValidationException()
    {
        var update = new DomainModel.Entry
        {
            Id = 99,
            ExternalLinks = [new DomainModel.EntryExternalLink { Id = 1, Type = DomainModel.ExternalLinkType.Fedlex, Url = "thisisaninvalidlink" }]
        };

        var ex = Assert.Throws<BusinessValidationException>(() => service.ValidateExternalLinks(new EntryUpdate { ExternalLinks = [new EntryModificationExternalLink()] }, update));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo("URL des externen Verweises 'thisisaninvalidlink' ist invalid"));
    }

    [Test]
    public async Task ValidateSubjectsShouldReturnEmptyList()
    {
        var empty = new EntryCreate();
        var result = await service.ValidateSubjects(empty);

        Assert.That(result, Is.EqualTo(new List<DomainModel.Subject>()));

        await subjectRepositoryMock.DidNotReceive().GetByIds(Arg.Any<IEnumerable<int>>());
    }

    [Test]
    public void ValidateSubjectsShouldThrow()
    {
        entryModification.SubjectIds = new List<int> { 1, 1 };
        Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidateSubjects(entryModification));
    }

    [Test]
    public async Task ShouldValidateSubjects()
    {
        var result = await service.ValidateSubjects(entryModification);

        Assert.That(result, Is.EqualTo(subjects));

        await subjectRepositoryMock.Received(1).GetByIds(entryModification.SubjectIds);
    }

    [Test]
    public void ShouldThrowBusinessValidationExceptionWhenValidateSubjectsFails()
    {
        subjectRepositoryMock.GetByIds(Arg.Any<IList<int>>()).Returns(new List<DomainModel.Subject> { subjects[0] });

        Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidateSubjects(entryModification));
    }

    [Test]
    public async Task ValidateDomainsShouldReturnEmptyList()
    {
        var empty = new EntryCreate();
        var result = await service.ValidateDomains(empty);

        Assert.That(result, Is.EqualTo(new List<DomainModel.Domain>()));

        await domainRepositoryMock.DidNotReceive().GetByIds(Arg.Any<IEnumerable<int>>());
    }

    [Test]
    public async Task ShouldValidateDomains()
    {
        var result = await service.ValidateDomains(entryModification);

        Assert.That(result, Is.EqualTo(domains));

        await domainRepositoryMock.Received(1).GetByIds(entryModification.DomainIds);
    }

    [Test]
    public void ShouldThrowBusinessValidationExceptionWhenValidateDomainsFails()
    {
        domainRepositoryMock.GetByIds(Arg.Any<IList<int>>()).Returns(new List<DomainModel.Domain> { domains[0] });

        Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidateDomains(entryModification));
    }

    [Test]
    public void ShouldThrowBusinessValidationExceptionWhenValidateDomainsDetectsNonMatchingDomains()
    {
        entryModification.SubjectIds = new List<int> { 1 };

        Assert.ThrowsAsync<BusinessValidationException>(async () => await service.ValidateDomains(entryModification));
    }

    [Test]
    public async Task ValidateAdminInfosLoadsAdminInfos()
    {
        entryModification.AdminInfos = adminInfoSummaries;

        var codes = new List<string> { "Test1", "Test2" };

        entry.AdminInfos = adminInfos;

        adminInfoRepositoryMock.GetByCodes(codes).Returns(adminInfos);

        var actual = await service.ValidateAdminInfos(entryModification);

        Assert.That(actual, Is.EqualTo(adminInfos));
    }

    [TestCaseSource(nameof(_authorizedGlossariesTestCases))]
    public void AuthorizeGlossaries_WithAuthorizedRole_DoesNotThrowForAuthorized(IEnumerable<int> glossaryIds, bool isAdmin, bool isSuperUser, bool isWriter, IEnumerable<int> authorizedGlossaries, IEnumerable<DomainModel.Glossary> existingGlossaries)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        authorizationServiceMock.IsSuperUser().Returns(isSuperUser);
        authorizationServiceMock.IsWriter().Returns(isWriter);
        authorizationServiceMock.GetAuthorizedGlossaryIds().Returns(authorizedGlossaries);

        Assert.DoesNotThrow(() => service.AuthorizeGlossaries(glossaryIds, existingGlossaries));
    }

    [TestCaseSource(nameof(_unauthorizedGlossariesTestCases))]
    public void AuthorizeGlossaries_WithUnauthorizedRole_ThrowsForbiddenException(IEnumerable<int> glossaryIds, bool isAdmin, bool isSuperUser, bool isWriter, IEnumerable<int> authorizedGlossaries, IEnumerable<DomainModel.Glossary> existingGlossaries)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        authorizationServiceMock.IsSuperUser().Returns(isSuperUser);
        authorizationServiceMock.IsWriter().Returns(isWriter);
        authorizationServiceMock.GetAuthorizedGlossaryIds().Returns(authorizedGlossaries);

        Assert.Throws<ForbiddenException>(() => service.AuthorizeGlossaries(glossaryIds, existingGlossaries));
    }

    [TestCaseSource(nameof(_authorizedCollectionTestCases))]
    public void AuthorizeCollection_WithAuthorizedRole_DoesNotThrowForAuthorized(int collectionId, bool isAdmin, bool isSuperUser, bool isWriter, IEnumerable<int> authorizedCollections, int? existingCollection)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        authorizationServiceMock.IsSuperUser().Returns(isSuperUser);
        authorizationServiceMock.IsWriter().Returns(isWriter);
        authorizationServiceMock.GetAuthorizedCollectionIds().Returns(authorizedCollections);

        Assert.DoesNotThrow(() => service.AuthorizeCollection(collectionId, existingCollection));
    }

    [TestCaseSource(nameof(_unauthorizedCollectionTestCases))]
    public void AuthorizeCollection_WithUnauthorizedRole_ThrowsForbiddenException(int collectionId, bool isAdmin, bool isSuperUser, bool isWriter, IEnumerable<int> authorizedCollections, int? existingCollection)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        authorizationServiceMock.IsSuperUser().Returns(isSuperUser);
        authorizationServiceMock.IsWriter().Returns(isWriter);
        authorizationServiceMock.GetAuthorizedCollectionIds().Returns(authorizedCollections);

        Assert.Throws<ForbiddenException>(() => service.AuthorizeCollection(collectionId, existingCollection));
    }

    private static readonly object[] _authorizedGlossariesTestCases =
    {
        new object[] { null, false, false, false, Array.Empty<int>(), null },
        new object[] { new[] { 1 }, true, false, false, Array.Empty<int>(), null },
        new object[] { new[] { 1, 2, 3 }, true, false, false, Array.Empty<int>(), null },
        new object[] { new[] { 1 }, false, true, false, Array.Empty<int>(), null },
        new object[] { new[] { 1, 2, 3 }, false, true, false, Array.Empty<int>(), null },
        new object[] { new[] { 1 }, false, false, true, new[] { 1 }, null },
        new object[] { new[] { 1 }, false, false, true, new[] { 3, 4, 1 }, null },
        new object[] { new[] { 1, 2, 3 }, false, false, true, new[] { 1, 3 }, new[] { new DomainModel.Glossary { Id = 2 } } },
        new object[] { new[] { 1 }, false, false, true, Array.Empty<int>(), new[] { new DomainModel.Glossary { Id = 1 } } },
        new object[] { new[] { 1, 2, 3, 4 }, false, false, true, new[] { 1, 3 }, new[] { new DomainModel.Glossary { Id = 2 }, new DomainModel.Glossary { Id = 4 } } },
    };

    private static readonly object[] _unauthorizedGlossariesTestCases =
    {
        new object[] { new[] { 1 }, false, false, false, Array.Empty<int>(), null },
        new object[] { new[] { 1 }, false, false, true, Array.Empty<int>(), null },
        new object[] { new[] { 2 }, false, false, true, new[] { 1 }, null },
        new object[] { new[] { 1, 2 }, false, false, true, new[] { 1, 3, 4 }, null },
        new object[] { new[] { 1, 2 }, false, false, true, Array.Empty<int>(), new[] { new DomainModel.Glossary { Id = 1 } } },
        new object[] { new[] { 1, 2, 3 }, false, false, true, new[] { 2 }, new[] { new DomainModel.Glossary { Id = 1 } } },
        new object[] { new[] { 1, 2, 3, 4 }, false, false, true, new[] { 1 }, new[] { new DomainModel.Glossary { Id = 2 }, new DomainModel.Glossary { Id = 4 } } },
    };

    private static readonly object[] _authorizedCollectionTestCases =
    {
        new object[] { 1, true, false, false, Array.Empty<int>(), null },
        new object[] { 1, false, true, false, Array.Empty<int>(), null },
        new object[] { 1, false, false, true, new[] { 1 }, null },
        new object[] { 1, false, false, true, Array.Empty<int>(), 1 },
    };

    private static readonly object[] _unauthorizedCollectionTestCases =
    {
        new object[] { 1, false, false, false, Array.Empty<int>(), null },
        new object[] { 1, false, false, true, Array.Empty<int>(), null },
        new object[] { 2, false, false, true, new[] { 1 }, null },
        new object[] { 2, false, false, true, Array.Empty<int>(), 1 },
        new object[] { 2, false, false, true, new[] { 3 }, 1 },
        new object[] { 2, false, false, true, new[] { 3, 4, 5 }, 1 },
    };
}
