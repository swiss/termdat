using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Business.Public.Api.Mapping.Actions;
using Bk.Termdat.Business.Public.Api.Mapping.Resolvers;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Mapping;

[TestFixture]
internal class PublicApiMappingProfileTests
{
    private IMapper mapper;
    private ITermdatUrlService termdatUrlServiceMock;
    private IBackendTextHelperService backendTextHelperServiceMock;
    private Fixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        termdatUrlServiceMock = Substitute.For<ITermdatUrlService>();
        backendTextHelperServiceMock = Substitute.For<IBackendTextHelperService>();

        using var loggerFactory = LoggerFactory.Create(_ => { });
        var services = new ServiceCollection();
        services.AddScoped(_ => termdatUrlServiceMock);
        services.AddScoped(_ => backendTextHelperServiceMock);
        services.AddTransient<EntryDetailUrlResolver>();
        services.AddTransient<EntryDetailStatusResolver>();
        services.AddTransient<EntryDetailReliabilityResolver>();
        services.AddTransient<EntryLanguageReferencesAction>();
        var provider = services.BuildServiceProvider();

        var config = new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new PublicApiMappingProfile());
        }, loggerFactory);
        mapper = config.CreateMapper();
    }

    [Test]
    public void ValidateMappingConfiguration()
    {
        var mapperConfig = mapper.ConfigurationProvider;
        mapperConfig.AssertConfigurationIsValid();
    }

    [Test]
    public void ShouldMapClassificationSummaryWithExistingLanguage()
    {
        var languageId = fixture.Create<int>();
        var classification = GenerateClassification();
        classification.Translations.First().LanguageId = languageId;

        var result = mapper.Map<ClassificationSummary>(classification, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, languageId); });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(classification.Id));
        Assert.That(result.Code, Is.EqualTo(classification.Code));
        Assert.That(result.Text, Is.EqualTo(classification.Translations.Single(t => t.LanguageId == languageId).Name));
    }

    [Test]
    public void ShouldMapClassificationSummaryWithNonExistingLanguage()
    {
        var languageId = fixture.Create<int>();
        var classification = GenerateClassification();

        var result = mapper.Map<ClassificationSummary>(classification, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, languageId); });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(classification.Id));
        Assert.That(result.Code, Is.EqualTo(classification.Code));
        Assert.That(result.Text, Is.Empty);
    }

    [Test]
    public void ShouldThrowOnMapClassificationSummaryWithoutMappingOptions()
    {
        Assert.Throws<AutoMapperMappingException>(() => mapper.Map<ClassificationSummary>(GenerateClassification()));
    }

    private Classification GenerateClassification()
    {
        var classification = new Classification
        {
            Id = fixture.Create<int>(),
            Code = fixture.Create<string>(),
            Translations = Enumerable.Range(1, 3).Select(_ => new ClassificationTranslation
            {
                Name = fixture.Create<string>(),
                LanguageId = fixture.Create<int>()
            }).ToList()
        };
        return classification;
    }

    [Test]
    public void ShouldMapCollectionSummaryWithExistingLanguage()
    {
        var languageId = fixture.Create<int>();
        var collection = GenerateCollection();
        collection.Translations.First().LanguageId = languageId;

        var result = mapper.Map<CollectionSummary>(collection, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, languageId); });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(collection.Id));
        Assert.That(result.Code, Is.EqualTo(collection.Code));
        Assert.That(result.Text, Is.EqualTo(collection.Translations.Single(t => t.LanguageId == languageId).Name));
    }

    [Test]
    public void ShouldMapCollectionSummaryWithNonExistingLanguage()
    {
        var languageId = fixture.Create<int>();
        var collection = GenerateCollection();

        var result = mapper.Map<CollectionSummary>(collection, options => { options.Items.Add(PublicApiMappingProfile.LanguageId, languageId); });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(collection.Id));
        Assert.That(result.Code, Is.EqualTo(collection.Code));
        Assert.That(result.Text, Is.Empty);
    }

    [Test]
    public void ShouldThrowOnMapCollectionSummaryWithoutMappingOptions()
    {
        Assert.Throws<AutoMapperMappingException>(() => mapper.Map<CollectionSummary>(GenerateCollection()));
    }

    private Collection GenerateCollection()
    {
        var classification = new Collection()
        {
            Id = fixture.Create<int>(),
            Code = fixture.Create<string>(),
            Translations = Enumerable.Range(1, 3).Select(_ => new CollectionTranslation
            {
                Name = fixture.Create<string>(),
                LanguageId = fixture.Create<int>()
            }).ToList()
        };
        return classification;
    }


    [Test]
    public void ShouldMapEntryDetail()
    {
        var languageId = fixture.Create<int>();
        var entry = fixture.Create<Entry>();

        entry.Office.Translations.First().LanguageId = languageId;
        entry.Collection.Translations.First().LanguageId = languageId;
        entry.Collection.Classification.Translations.First().LanguageId = languageId;
        entry.Subjects.ToList().ForEach(s => s.Translations.First().LanguageId = languageId);
        var domain = entry.Domains.First();
        domain.Translations.First().LanguageId = languageId;
        domain.SubjectId = entry.Subjects.First().Id;
        var firstEntryLanguage = entry.Translations.First();
        firstEntryLanguage.LanguageId = languageId;
        firstEntryLanguage.Sequence = 1;

        var url = fixture.Create<string>();
        termdatUrlServiceMock.GetTermdatUrl(entry.Id).Returns(url);

        var languageCode = fixture.Create<string>();
        var statusText = fixture.Create<string>();
        backendTextHelperServiceMock.GetStatusText(entry.Status, languageCode).Returns(statusText);
        var reliabilityText = fixture.Create<string>();
        backendTextHelperServiceMock.GetReliabilityText(entry.Reliability, languageCode).Returns(reliabilityText);
        Assert.That(entry.Translations, Is.Not.Null);
        Assert.That(entry.Translations, Is.Not.Empty);
        Assert.That(entry.Translations.All(t => t.Language != null), Is.True);
        var result = mapper.Map<EntryDetail>(entry, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, languageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, languageCode);
        });

        AssertEntryDetail(result, entry, url, statusText, reliabilityText, languageId);

        foreach (var subjectSummary in result.Subject)
        {
            var subject = entry.Subjects.SingleOrDefault(s => s.Id == subjectSummary.Id);
            Assert.That(subject, Is.Not.Null);
            Assert.That(subjectSummary.Text, Is.EqualTo(subject.Translations.Single(t => t.LanguageId == languageId).Name));
        }

        var firstSubjectSummary = result.Subject.First();
        var domainSummary = firstSubjectSummary.Domains.Single();
        Assert.That(domainSummary.Id, Is.EqualTo(domain.Id));
        Assert.That(domainSummary.Text, Is.EqualTo(domain.Translations.Single(t => t.LanguageId == languageId).Name));

        foreach (var languageDetail in result.LanguageDetails)
        {
            var entryLanguage = entry.Translations.Single(t => t.Id == languageDetail.Id);
            Assert.That(languageDetail.LanguageIsoCode, Is.EqualTo(entryLanguage.Language.IsoCode));
            Assert.That(languageDetail.Sequence, Is.EqualTo(entryLanguage.Sequence));
            Assert.That(languageDetail.Terminus, Is.EqualTo(entryLanguage.Terminus));
            Assert.That(languageDetail.TerminusSource, Is.EqualTo(entryLanguage.TerminusSource));
            Assert.That(languageDetail.Name, Is.EqualTo(entryLanguage.Name));
            Assert.That(languageDetail.NameSource, Is.EqualTo(entryLanguage.NameSource));
            Assert.That(languageDetail.Abbreviation, Is.EqualTo(entryLanguage.Abbreviation));
            Assert.That(languageDetail.AbbreviationSource, Is.EqualTo(entryLanguage.AbbreviationSource));
            Assert.That(languageDetail.Phraseology, Is.EqualTo(entryLanguage.Phraseology));
            Assert.That(languageDetail.PhraseologySource, Is.EqualTo(entryLanguage.PhraseologySource));
            Assert.That(languageDetail.Definition, Is.EqualTo(entryLanguage.Definition));
            Assert.That(languageDetail.DefinitionSource, Is.EqualTo(entryLanguage.DefinitionSource));
            Assert.That(languageDetail.Note, Is.EqualTo(entryLanguage.Note));
            Assert.That(languageDetail.NoteSource, Is.EqualTo(entryLanguage.NoteSource));
            Assert.That(languageDetail.Context, Is.EqualTo(entryLanguage.Context));
            Assert.That(languageDetail.ContextSource, Is.EqualTo(entryLanguage.ContextSource));
            Assert.That(languageDetail.Country, Is.EqualTo(entryLanguage.Country));
            Assert.That(languageDetail.Metadata, Is.EqualTo(entryLanguage.Metadata));
            Assert.That(languageDetail.MetadataSource, Is.EqualTo(entryLanguage.MetadataSource));
        }
    }

    private static void AssertEntryDetail(EntryDetail result, Entry entry, string url, string statusText,
        string reliabilityText, int languageId)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entry.Id));
        Assert.That(result.Url, Is.EqualTo(url));
        Assert.That(result.Status.Code, Is.EqualTo(entry.Status.ToString("D")));
        Assert.That(result.Status.Text, Is.EqualTo(statusText));
        Assert.That(result.Reliability.Code, Is.EqualTo(entry.Reliability.ToString("D")));
        Assert.That(result.Reliability.Text, Is.EqualTo(reliabilityText));
        Assert.That(result.Office.Id, Is.EqualTo(entry.Office.Id));
        Assert.That(result.Office.Code, Is.EqualTo(entry.Office.Code));
        Assert.That(result.Office.Text, Is.EqualTo(entry.Office.Translations.Single(t => t.LanguageId == languageId).Name));
        Assert.That(result.Collection.Id, Is.EqualTo(entry.Collection.Id));
        Assert.That(result.Collection.Code, Is.EqualTo(entry.Collection.Code));
        Assert.That(result.Collection.Text,
            Is.EqualTo(entry.Collection.Translations.Single(t => t.LanguageId == languageId).Name));
        Assert.That(result.Classification.Id, Is.EqualTo(entry.Collection.Classification.Id));
        Assert.That(result.Classification.Code, Is.EqualTo(entry.Collection.Classification.Code));
        Assert.That(result.Classification.Text,
            Is.EqualTo(entry.Collection.Classification.Translations.Single(t => t.LanguageId == languageId).Name));
    }

    [Test]
    public void ShouldMapEntryDetailWithoutTranslations()
    {
        var languageId = fixture.Create<int>();
        var entry = fixture.Create<Entry>();

        entry.Office.Translations.First().LanguageId = languageId;
        entry.Collection.Translations.First().LanguageId = languageId;
        entry.Collection.Classification.Translations.First().LanguageId = languageId;
        entry.Subjects.ToList().ForEach(s => s.Translations.First().LanguageId = languageId);
        var domain = entry.Domains.First();
        domain.Translations.First().LanguageId = languageId;
        domain.SubjectId = entry.Subjects.First().Id;
        entry.Translations = null;

        var url = fixture.Create<string>();
        termdatUrlServiceMock.GetTermdatUrl(entry.Id).Returns(url);

        var languageCode = fixture.Create<string>();
        var statusText = fixture.Create<string>();
        backendTextHelperServiceMock.GetStatusText(entry.Status, languageCode).Returns(statusText);
        var reliabilityText = fixture.Create<string>();
        backendTextHelperServiceMock.GetReliabilityText(entry.Reliability, languageCode).Returns(reliabilityText);

        var result = mapper.Map<EntryDetail>(entry, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, languageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, languageCode);
        });


        AssertEntryDetail(result, entry, url, statusText, reliabilityText, languageId);
        Assert.That(result.LanguageDetails, Is.Null);
    }

    [Test]
    public void ShuoldMapEntryReferences()
    {
        var languageId = fixture.Create<int>();
        var entry = fixture.Create<Entry>();

        entry.Office.Translations.First().LanguageId = languageId;
        entry.Collection.Translations.First().LanguageId = languageId;
        entry.Collection.Classification.Translations.First().LanguageId = languageId;
        entry.Subjects.ToList().ForEach(s => s.Translations.First().LanguageId = languageId);
        var domain = entry.Domains.First();
        domain.Translations.First().LanguageId = languageId;
        domain.SubjectId = entry.Subjects.First().Id;
        var languageCode = fixture.Create<string>();
        entry.Translations.ToList().ForEach(t =>
        {
            t.Language.IsoCode = languageCode;
            t.LanguageId = languageId;
            t.Sequence = 1;
            t.Entry = entry;
        });

        var referencedEntryId = fixture.Create<int>();
        var terminus = fixture.Create<string>();
        var referenceType = fixture.Create<ReferenceType>();
        entry.References = new List<EntryReference>
        {
            new()
            {
                EntryId = referencedEntryId,
                Type = referenceType,
                ReferencedEntry = new Entry
                {
                    Translations = new List<EntryLanguage>
                    {
                        new()
                        {
                            LanguageId = languageId,
                            Terminus = terminus
                        }
                    }
                }
            }
        };

        var url = fixture.Create<string>();
        termdatUrlServiceMock.GetTermdatUrl(entry.Id).Returns(url);
        var referencedEntryUrl = fixture.Create<string>();
        termdatUrlServiceMock.GetTermdatUrl(referencedEntryId).Returns(referencedEntryUrl);

        var statusText = fixture.Create<string>();
        backendTextHelperServiceMock.GetStatusText(entry.Status, languageCode).Returns(statusText);
        var reliabilityText = fixture.Create<string>();
        backendTextHelperServiceMock.GetReliabilityText(entry.Reliability, languageCode).Returns(reliabilityText);
        var referenceTypeText = fixture.Create<string>();
        backendTextHelperServiceMock.GetReferenceTypeText(referenceType, languageCode).Returns(referenceTypeText);

        var result = mapper.Map<EntryDetail>(entry, options =>
        {
            options.Items.Add(PublicApiMappingProfile.LanguageId, languageId);
            options.Items.Add(PublicApiMappingProfile.LanguageIsoCode, languageCode);
        });


        AssertEntryDetail(result, entry, url, statusText, reliabilityText, languageId);
        var referenceSummaries = result.LanguageDetails.Where(ld => ld.References != null).SelectMany(ld => ld.References).ToList();
        Assert.That(referenceSummaries, Has.Count.EqualTo(entry.Translations.Count));
        foreach (var entryReferenceSummary in referenceSummaries)
        {
            Assert.That(entryReferenceSummary.Text, Is.EqualTo(terminus));
            Assert.That(entryReferenceSummary.Url, Is.EqualTo(referencedEntryUrl));
            Assert.That(entryReferenceSummary.Type, Is.EqualTo(referenceTypeText));
        }
    }
}
