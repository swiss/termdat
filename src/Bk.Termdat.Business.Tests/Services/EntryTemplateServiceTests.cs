using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class EntryTemplateServiceTests
{
    private EntryTemplateService entryTemplateService;
    private IEntryValidationService entryValidationService;
    private IEntryTemplateRepository repository;
    private IMapper mapper;
    private IIdentityService identityService;
    private IUserAccountRepository userAccountRepository;
    private IOfficeRepository officeRepository;
    private ICollectionRepository collectionRepository;
    private IUnitOfWork unitOfWork;

    private EntryTemplateCreate entryTemplateCreate;
    private EntryTemplateUpdate entryTemplateUpdate;
    private Entry entry;
    private IEnumerable<EntryTemplateSummary> entryTemplateSummaries;
    private IList<EntryTemplate> entryTemplates;
    private EntryModificationDetail entryModificationDetail;
    private Office office;
    private Collection collection;

    [SetUp]
    public void SetUp()
    {
        repository = Substitute.For<IEntryTemplateRepository>();
        entryValidationService = Substitute.For<IEntryValidationService>();
        mapper = Substitute.For<IMapper>();
        identityService = Substitute.For<IIdentityService>();
        userAccountRepository = Substitute.For<IUserAccountRepository>();
        officeRepository = Substitute.For<IOfficeRepository>();
        collectionRepository = Substitute.For<ICollectionRepository>();
        unitOfWork = Substitute.For<IUnitOfWork>();
        entryTemplateService = new EntryTemplateService(entryValidationService, repository, mapper, identityService, userAccountRepository, officeRepository, collectionRepository, unitOfWork);

        entryTemplates = new List<EntryTemplate>
        {
            new()
            {
                Id = 1,
                Name = "123",
                Template = GetTestEntryJson()
            },
            new()
            {
                Id = 2,
                Name = "abc",
                Template = "\"Identification\":336,\"Comment\":\"CommentTest\",\"Subjects\":[{\"Name\":\"SubjectNameTest\"}]"
            }
        };

        entryTemplateCreate = new EntryTemplateCreate
        {
            TemplateName = "Template1",
            Identification = 123,
            Comment = "CommentTest",
            SubjectIds = new List<int> { 11 }
        };

        entryTemplateUpdate = new EntryTemplateUpdate
        {
            Id = 333,
            Identification = 123,
            Comment = "CommentTestMut",
            SubjectIds = new List<int> { 11, 12 }
        };

        entry = new Entry
        {
            Id = 1,
            OfficeId = 1,
            CollectionId = 1,
        };

        entryModificationDetail = new EntryModificationDetail
        {
            Id = 1,
            Reliability = Business.Resource.Models.Reliability.Tentative,
            Status = Business.Resource.Models.Status.InProgress,
            Comment = "Test-Kommentar",
            AdminInfos = new List<AdminInfoSummary>
            {
                new() { Id = 1, Code = "Brot" }
            },
            Collection = new CollectionSummary
            {
                Id = 123,
                Code = "ALLG14"
            },
            Domains = new List<DomainSummary>
            {
                new() { Id= 240, Name = "Chemische Stoffe"}
            },
            Glossaries = new List<GlossarySummary>
            {
                new() { Id = 1, Code = "G01"}
            },
            LanguageDetails = new List<EntryModificationLanguageDetail>
            {
                new() { Id = 1226013, Name = "Name D", NameSource = "Quelle Name D", Terminus = "Terminus D", TerminusSource = "Quelle Terminus D" }
            },
            Subjects = new List<SubjectSummary>
            {
                new() { Id= 240, Name = "Chemie"}
            }
        };

        entryTemplateSummaries = entryTemplates.Select(et => new EntryTemplateSummary
        {
            Id = et.Id,
            TemplateName = et.Name,
        });

        office = new Office
        {
            Id = 1
        };

        collection = new Collection
        {
            Id = 1
        };

        mapper.Map<Entry>(entryTemplateCreate).Returns(entry);
        mapper.Map<Entry>(entryTemplateUpdate).Returns(entry);
        mapper.Map<IEnumerable<EntryTemplateSummary>>(entryTemplates).Returns(entryTemplateSummaries);
        mapper.Map<EntryModificationDetail>(Arg.Any<Entry>()).Returns(entryModificationDetail);

        repository.TemplateNameExistsForUser(1, "Test").Returns(true);
        repository.GetForUser(1).Returns(entryTemplates);
        repository.GetById(333).Returns(entryTemplates[0]);

        officeRepository.GetById(1).Returns(office);
        collectionRepository.GetById(1).Returns(collection);
        identityService.GetCurrentEmail().Returns("test@test.ch");

        entryValidationService.ValidateRelatedCollections(Arg.Any<IEntryModification>()).Returns(new List<Collection>
        {
            new() { Id = 11, Translations = new List<CollectionTranslation> { new() { Id = 111, Name = "Collection 1"} } }
        });
        entryValidationService.ValidateSubjects(Arg.Any<IEntryModification>()).Returns(new List<Subject>
        {
            new() { Id = 22, Translations = new List<SubjectTranslation> { new() { Id = 222, Name = "Subject 1"} } }
        });
        entryValidationService.ValidateDomains(Arg.Any<IEntryModification>()).Returns(new List<Domain.Models.Domain>
        {
            new() { Id = 33, Translations = new List<DomainTranslation> { new() { Id = 333, Name = "Domain 1"} } }
        });
        entryValidationService.ValidateGlossaries(Arg.Any<IEntryModification>()).Returns(new List<Glossary>
        {
            new() { Id = 44, Code = "Glossary" }
        });
        entryValidationService.ValidateReferences(Arg.Any<IEntryModification>(), Arg.Any<Entry>()).Returns(new List<EntryReference>
        {
            new() { Id = 55, ReferencedEntryId = 555 }
        });
        entryValidationService.ValidateAdminInfos(Arg.Any<IEntryModification>()).Returns(new List<AdminInfo>
        {
            new() { Id = 66, Code = "AdminInfo" }
        });

        var user = new UserAccount { Id = 1, };
        userAccountRepository.GetByEmail(Arg.Any<string>()).Returns(user);
    }

    [TearDown]
    public void TearDown()
    {
        repository.ClearSubstitute();
        mapper.ClearSubstitute();
        identityService.ClearSubstitute();
        unitOfWork.ClearSubstitute();
    }

    [Test]
    public async Task Create_WhenCalled_ShouldSucceed()
    {
        EntryTemplate result = null!;
        repository.When(x => x.Create(Arg.Any<EntryTemplate>()))
            .Do(x => result = x.Arg<EntryTemplate>());

        await entryTemplateService.Create(entryTemplateCreate);

        identityService.Received(1).GetCurrentEmail();
        await repository.Received(1).Create(Arg.Any<EntryTemplate>());
        await unitOfWork.Received(1).SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Template, Contains.Substring("\"OfficeId\": 1,"));
        Assert.That(result.Template, Contains.Substring("\"CollectionId\": 1,"));
        Assert.That(result.Template, Contains.Substring("\"Name\": \"Domain 1\","));
        Assert.That(result.Template, Contains.Substring("\"OfficeId\": 1,"));
        Assert.That(result.Template, Contains.Substring("\"Name\": \"Collection 1\""));
        Assert.That(result.Template, Contains.Substring("\"ReferencedEntryId\": 555,"));
        Assert.That(result.Template, Contains.Substring("\"Code\": \"AdminInfo\","));
        Assert.That(result.CreatedBy, Is.EqualTo("test@test.ch"));
        Assert.That(result.ModifiedBy, Is.EqualTo("test@test.ch"));
    }

    [Test]
    public async Task Create_WhenCalledWithExistingName_ThrowsException()
    {
        entryTemplateCreate.TemplateName = "Test";

        Assert.ThrowsAsync<BusinessValidationException>(() => entryTemplateService.Create(entryTemplateCreate));
        await repository.Received(1).TemplateNameExistsForUser(1, "Test");
    }

    [Test]
    public async Task Update_WhenCalledWithValidTemplate_ShouldSucceed()
    {
        await entryTemplateService.Update(entryTemplateUpdate);

        mapper.Received(1).Map<Entry>(entryTemplateUpdate);
        identityService.Received(1).GetCurrentEmail();
        repository.Received(1).Update(entryTemplates[0]);
        await unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Update_WhenCalledWithInvalidTemplate_ShouldThrow()
    {
        entryTemplateUpdate.Id = 0;

        Assert.ThrowsAsync<EntityNotFoundException>(() => entryTemplateService.Update(entryTemplateUpdate));
        await repository.Received(1).GetById(0);
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldSucceed()
    {
        await entryTemplateService.Delete(333);

        await repository.Received(1).GetById(Arg.Is(333));
        repository.Received(1).Delete(Arg.Any<EntryTemplate>());
        await unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Delete_WhenCalledWithInvalidId_ShouldThrow()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(() => entryTemplateService.Delete(0));

        await repository.Received(1).GetById(Arg.Is(0));
        repository.Received(0).Delete(Arg.Any<EntryTemplate>());
        await unitOfWork.Received(0).SaveChangesAsync();
    }

    [Test]
    public async Task GetForUser_WhenCalled_ShouldSucceed()
    {
        var result = await entryTemplateService.GetForUser();

        Assert.That(result, Is.EqualTo(entryTemplateSummaries));
    }

    [Test]
    public async Task GetEntryByTemplate_WhenCalled_ShouldSucceed()
    {
        var result = await entryTemplateService.GetTemplateById(333);

        await repository.Received(1).GetById(Arg.Is(333));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Reliability, Is.EqualTo(Business.Resource.Models.Reliability.Tentative));
        Assert.That(result.Status, Is.EqualTo(Business.Resource.Models.Status.InProgress));
        Assert.That(result.Comment, Is.EqualTo("Test-Kommentar"));
        Assert.That(result.AdminInfos[0].Code, Is.EqualTo("Brot"));
        Assert.That(result.Collection.Code, Is.EqualTo("ALLG14"));
        Assert.That(result.Domains[0].Id, Is.EqualTo(240));
        Assert.That(result.Domains[0].Name, Is.EqualTo("Chemische Stoffe"));
        Assert.That(result.Glossaries[0].Id, Is.EqualTo(1));
        Assert.That(result.Glossaries[0].Code, Is.EqualTo("G01"));
        Assert.That(result.LanguageDetails[0].Id, Is.EqualTo(1226013));
        Assert.That(result.LanguageDetails[0].Name, Is.EqualTo("Name D"));
        Assert.That(result.LanguageDetails[0].NameSource, Is.EqualTo("Quelle Name D"));
        Assert.That(result.LanguageDetails[0].Terminus, Is.EqualTo("Terminus D"));
        Assert.That(result.LanguageDetails[0].TerminusSource, Is.EqualTo("Quelle Terminus D"));
        Assert.That(result.Subjects[0].Id, Is.EqualTo(240));
        Assert.That(result.Subjects[0].Name, Is.EqualTo("Chemie"));
    }

    private string GetTestEntryJson()
    {
        return "{\r\n  \"$id\": \"1\",\r\n  \"Status\": 0,\r\n  \"Reliability\": 1,\r\n  \"Identification\": null,\r\n  \"Comment\": \"Test Comment\",\r\n  \"AdminInfo\": null,\r\n  \"Deleted\": false,\r\n  \"OfficeId\": 1,\r\n  \"CollectionId\": 123,\r\n  \"ConceptSystemPositionId\": null,\r\n  \"Locked\": null,\r\n  \"LockedBy\": null,\r\n  \"Collection\": null,\r\n  \"Office\": null,\r\n  \"Picture\": null,\r\n  \"ConceptSystemPosition\": null,\r\n  \"Subjects\": {\r\n    \"$id\": \"2\",\r\n    \"$values\": [\r\n      {\r\n        \"$id\": \"3\",\r\n        \"Translations\": {\r\n          \"$id\": \"4\",\r\n          \"$values\": [\r\n            {\r\n              \"$id\": \"5\",\r\n              \"LanguageId\": 2,\r\n              \"Language\": {\r\n                \"$id\": \"6\",\r\n                \"IsoCode\": \"DE\",\r\n                \"IsUiLanguage\": true,\r\n                \"IsSystemLanguage\": true,\r\n                \"SortIndex\": 1,\r\n                \"Translations\": null,\r\n                \"Created\": \"2013-02-22T22:07:39.987Z\",\r\n                \"CreatedBy\": \"System\",\r\n                \"Modified\": \"2013-02-22T22:07:39.987Z\",\r\n                \"ModifiedBy\": \"System\",\r\n                \"Id\": 2,\r\n                \"ConcurrencyToken\": 594\r\n              },\r\n              \"Name\": \"ENERGIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"3\"\r\n              },\r\n              \"SubjectId\": 4,\r\n              \"Id\": 16,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"7\",\r\n              \"LanguageId\": 6,\r\n              \"Language\": {\r\n                \"$id\": \"8\",\r\n                \"IsoCode\": \"FR\",\r\n                \"IsUiLanguage\": true,\r\n                \"IsSystemLanguage\": true,\r\n                \"SortIndex\": 2,\r\n                \"Translations\": null,\r\n                \"Created\": \"2013-02-22T22:07:39.987Z\",\r\n                \"CreatedBy\": \"System\",\r\n                \"Modified\": \"2013-02-22T22:07:39.987Z\",\r\n                \"ModifiedBy\": \"System\",\r\n                \"Id\": 6,\r\n                \"ConcurrencyToken\": 594\r\n              },\r\n              \"Name\": \"\\u00C9NERGIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"3\"\r\n              },\r\n              \"SubjectId\": 4,\r\n              \"Id\": 17,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"9\",\r\n              \"LanguageId\": 7,\r\n              \"Language\": {\r\n                \"$id\": \"10\",\r\n                \"IsoCode\": \"IT\",\r\n                \"IsUiLanguage\": true,\r\n                \"IsSystemLanguage\": true,\r\n                \"SortIndex\": 3,\r\n                \"Translations\": null,\r\n                \"Created\": \"2013-02-22T22:07:39.987Z\",\r\n                \"CreatedBy\": \"System\",\r\n                \"Modified\": \"2013-02-22T22:07:39.987Z\",\r\n                \"ModifiedBy\": \"System\",\r\n                \"Id\": 7,\r\n                \"ConcurrencyToken\": 594\r\n              },\r\n              \"Name\": \"ENERGIA\",\r\n              \"Subject\": {\r\n                \"$ref\": \"3\"\r\n              },\r\n              \"SubjectId\": 4,\r\n              \"Id\": 18,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"11\",\r\n              \"LanguageId\": 3,\r\n              \"Language\": {\r\n                \"$id\": \"12\",\r\n                \"IsoCode\": \"EN\",\r\n                \"IsUiLanguage\": true,\r\n                \"IsSystemLanguage\": true,\r\n                \"SortIndex\": 5,\r\n                \"Translations\": null,\r\n                \"Created\": \"2013-02-22T22:07:39.987Z\",\r\n                \"CreatedBy\": \"System\",\r\n                \"Modified\": \"2013-02-22T22:07:39.987Z\",\r\n                \"ModifiedBy\": \"System\",\r\n                \"Id\": 3,\r\n                \"ConcurrencyToken\": 594\r\n              },\r\n              \"Name\": \"ENERGY\",\r\n              \"Subject\": {\r\n                \"$ref\": \"3\"\r\n              },\r\n              \"SubjectId\": 4,\r\n              \"Id\": 19,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"13\",\r\n              \"LanguageId\": 8,\r\n              \"Language\": {\r\n                \"$id\": \"14\",\r\n                \"IsoCode\": \"RM\",\r\n                \"IsUiLanguage\": true,\r\n                \"IsSystemLanguage\": true,\r\n                \"SortIndex\": 4,\r\n                \"Translations\": null,\r\n                \"Created\": \"2013-02-22T22:07:39.987Z\",\r\n                \"CreatedBy\": \"System\",\r\n                \"Modified\": \"2013-02-22T22:07:39.987Z\",\r\n                \"ModifiedBy\": \"System\",\r\n                \"Id\": 8,\r\n                \"ConcurrencyToken\": 594\r\n              },\r\n              \"Name\": \"RM_ENERGIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"3\"\r\n              },\r\n              \"SubjectId\": 4,\r\n              \"Id\": 20,\r\n              \"ConcurrencyToken\": 603\r\n            }\r\n          ]\r\n        },\r\n        \"Domains\": null,\r\n        \"Entries\": null,\r\n        \"LenochClassificationSubjectDomainMappings\": null,\r\n        \"Created\": \"2022-06-09T14:50:03.689656Z\",\r\n        \"CreatedBy\": \"system@termdat.ch\",\r\n        \"Modified\": \"2022-06-09T14:50:03.689656Z\",\r\n        \"ModifiedBy\": \"system@termdat.ch\",\r\n        \"Id\": 4,\r\n        \"ConcurrencyToken\": 604\r\n      },\r\n      {\r\n        \"$id\": \"15\",\r\n        \"Translations\": {\r\n          \"$id\": \"16\",\r\n          \"$values\": [\r\n            {\r\n              \"$id\": \"17\",\r\n              \"LanguageId\": 2,\r\n              \"Language\": {\r\n                \"$ref\": \"6\"\r\n              },\r\n              \"Name\": \"CHEMIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"SubjectId\": 33,\r\n              \"Id\": 161,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"18\",\r\n              \"LanguageId\": 6,\r\n              \"Language\": {\r\n                \"$ref\": \"8\"\r\n              },\r\n              \"Name\": \"CHIMIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"SubjectId\": 33,\r\n              \"Id\": 162,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"19\",\r\n              \"LanguageId\": 7,\r\n              \"Language\": {\r\n                \"$ref\": \"10\"\r\n              },\r\n              \"Name\": \"CHIMICA\",\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"SubjectId\": 33,\r\n              \"Id\": 163,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"20\",\r\n              \"LanguageId\": 3,\r\n              \"Language\": {\r\n                \"$ref\": \"12\"\r\n              },\r\n              \"Name\": \"CHEMISTRY\",\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"SubjectId\": 33,\r\n              \"Id\": 164,\r\n              \"ConcurrencyToken\": 603\r\n            },\r\n            {\r\n              \"$id\": \"21\",\r\n              \"LanguageId\": 8,\r\n              \"Language\": {\r\n                \"$ref\": \"14\"\r\n              },\r\n              \"Name\": \"RM_CHEMIE\",\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"SubjectId\": 33,\r\n              \"Id\": 165,\r\n              \"ConcurrencyToken\": 603\r\n            }\r\n          ]\r\n        },\r\n        \"Domains\": {\r\n          \"$id\": \"22\",\r\n          \"$values\": [\r\n            {\r\n              \"$id\": \"23\",\r\n              \"SubjectId\": 33,\r\n              \"Subject\": {\r\n                \"$ref\": \"15\"\r\n              },\r\n              \"Translations\": {\r\n                \"$id\": \"24\",\r\n                \"$values\": [\r\n                  {\r\n                    \"$id\": \"25\",\r\n                    \"LanguageId\": 2,\r\n                    \"Language\": {\r\n                      \"$ref\": \"6\"\r\n                    },\r\n                    \"Name\": \"Chemische Stoffe\",\r\n                    \"Domain\": {\r\n                      \"$ref\": \"23\"\r\n                    },\r\n                    \"DomainId\": 240,\r\n                    \"Id\": 1196,\r\n                    \"ConcurrencyToken\": 584\r\n                  },\r\n                  {\r\n                    \"$id\": \"26\",\r\n                    \"LanguageId\": 6,\r\n                    \"Language\": {\r\n                      \"$ref\": \"8\"\r\n                    },\r\n                    \"Name\": \"Produits chimiques\",\r\n                    \"Domain\": {\r\n                      \"$ref\": \"23\"\r\n                    },\r\n                    \"DomainId\": 240,\r\n                    \"Id\": 1197,\r\n                    \"ConcurrencyToken\": 584\r\n                  },\r\n                  {\r\n                    \"$id\": \"27\",\r\n                    \"LanguageId\": 7,\r\n                    \"Language\": {\r\n                      \"$ref\": \"10\"\r\n                    },\r\n                    \"Name\": \"Prodotti chimici\",\r\n                    \"Domain\": {\r\n                      \"$ref\": \"23\"\r\n                    },\r\n                    \"DomainId\": 240,\r\n                    \"Id\": 1198,\r\n                    \"ConcurrencyToken\": 584\r\n                  },\r\n                  {\r\n                    \"$id\": \"28\",\r\n                    \"LanguageId\": 3,\r\n                    \"Language\": {\r\n                      \"$ref\": \"12\"\r\n                    },\r\n                    \"Name\": \"Chemicals\",\r\n                    \"Domain\": {\r\n                      \"$ref\": \"23\"\r\n                    },\r\n                    \"DomainId\": 240,\r\n                    \"Id\": 1199,\r\n                    \"ConcurrencyToken\": 584\r\n                  },\r\n                  {\r\n                    \"$id\": \"29\",\r\n                    \"LanguageId\": 8,\r\n                    \"Language\": {\r\n                      \"$ref\": \"14\"\r\n                    },\r\n                    \"Name\": \"RM_Chemische Stoffe\",\r\n                    \"Domain\": {\r\n                      \"$ref\": \"23\"\r\n                    },\r\n                    \"DomainId\": 240,\r\n                    \"Id\": 1200,\r\n                    \"ConcurrencyToken\": 584\r\n                  }\r\n                ]\r\n              },\r\n              \"Entries\": null,\r\n              \"LenochClassificationSubjectDomainMappings\": null,\r\n              \"Created\": \"2022-06-09T14:50:03.689762Z\",\r\n              \"CreatedBy\": \"system@termdat.ch\",\r\n              \"Modified\": \"2022-06-09T14:50:03.689762Z\",\r\n              \"ModifiedBy\": \"system@termdat.ch\",\r\n              \"Id\": 240,\r\n              \"ConcurrencyToken\": 585\r\n            }\r\n          ]\r\n        },\r\n        \"Entries\": null,\r\n        \"LenochClassificationSubjectDomainMappings\": null,\r\n        \"Created\": \"2022-06-09T14:50:03.689763Z\",\r\n        \"CreatedBy\": \"system@termdat.ch\",\r\n        \"Modified\": \"2022-06-09T14:50:03.689763Z\",\r\n        \"ModifiedBy\": \"system@termdat.ch\",\r\n        \"Id\": 33,\r\n        \"ConcurrencyToken\": 604\r\n      }\r\n    ]\r\n  },\r\n  \"Domains\": {\r\n    \"$id\": \"30\",\r\n    \"$values\": [\r\n      {\r\n        \"$ref\": \"23\"\r\n      }\r\n    ]\r\n  },\r\n  \"RelatedCollections\": {\r\n    \"$id\": \"31\",\r\n    \"$values\": [\r\n      {\r\n        \"$id\": \"32\",\r\n        \"Code\": \"ABR22\",\r\n        \"Documentation\": null,\r\n        \"IsObsolete\": false,\r\n        \"Active\": true,\r\n        \"ActiveApi\": true,\r\n        \"Deleted\": false,\r\n        \"ExportLindas\": false,\r\n        \"ClassificationId\": 12,\r\n        \"ConceptSystem\": null,\r\n        \"Classification\": {\r\n          \"$id\": \"33\",\r\n          \"Code\": \"RECH\",\r\n          \"Translations\": null,\r\n          \"Collections\": {\r\n            \"$id\": \"34\",\r\n            \"$values\": [\r\n              {\r\n                \"$ref\": \"32\"\r\n              }\r\n            ]\r\n          },\r\n          \"Created\": \"2022-06-09T14:50:05.049314Z\",\r\n          \"CreatedBy\": \"system@termdat.ch\",\r\n          \"Modified\": \"2022-06-09T14:50:05.049314Z\",\r\n          \"ModifiedBy\": \"system@termdat.ch\",\r\n          \"Id\": 12,\r\n          \"ConcurrencyToken\": 577\r\n        },\r\n        \"Translations\": {\r\n          \"$id\": \"35\",\r\n          \"$values\": [\r\n            {\r\n              \"$id\": \"36\",\r\n              \"Name\": \"Abk\\u00FCrzungen der Titel der Bundeserlasse\",\r\n              \"LanguageId\": 2,\r\n              \"CollectionId\": 90,\r\n              \"Language\": {\r\n                \"$ref\": \"6\"\r\n              },\r\n              \"Collection\": {\r\n                \"$ref\": \"32\"\r\n              },\r\n              \"Id\": 16,\r\n              \"ConcurrencyToken\": 579\r\n            },\r\n            {\r\n              \"$id\": \"37\",\r\n              \"Name\": \"EN: Abk\\u00FCrzungen der Titel der Bundeserlasse\",\r\n              \"LanguageId\": 3,\r\n              \"CollectionId\": 90,\r\n              \"Language\": {\r\n                \"$ref\": \"12\"\r\n              },\r\n              \"Collection\": {\r\n                \"$ref\": \"32\"\r\n              },\r\n              \"Id\": 17,\r\n              \"ConcurrencyToken\": 579\r\n            },\r\n            {\r\n              \"$id\": \"38\",\r\n              \"Name\": \"Abr\\u00E9viations des titres des actes l\\u00E9gislatifs f\\u00E9d\\u00E9raux\",\r\n              \"LanguageId\": 6,\r\n              \"CollectionId\": 90,\r\n              \"Language\": {\r\n                \"$ref\": \"8\"\r\n              },\r\n              \"Collection\": {\r\n                \"$ref\": \"32\"\r\n              },\r\n              \"Id\": 18,\r\n              \"ConcurrencyToken\": 579\r\n            },\r\n            {\r\n              \"$id\": \"39\",\r\n              \"Name\": \"Abbreviazioni dei titoli degli atti legislativi federali\",\r\n              \"LanguageId\": 7,\r\n              \"CollectionId\": 90,\r\n              \"Language\": {\r\n                \"$ref\": \"10\"\r\n              },\r\n              \"Collection\": {\r\n                \"$ref\": \"32\"\r\n              },\r\n              \"Id\": 19,\r\n              \"ConcurrencyToken\": 579\r\n            },\r\n            {\r\n              \"$id\": \"40\",\r\n              \"Name\": \"Abreviaziuns dals titels dals decrets federals\",\r\n              \"LanguageId\": 8,\r\n              \"CollectionId\": 90,\r\n              \"Language\": {\r\n                \"$ref\": \"14\"\r\n              },\r\n              \"Collection\": {\r\n                \"$ref\": \"32\"\r\n              },\r\n              \"Id\": 20,\r\n              \"ConcurrencyToken\": 579\r\n            }\r\n          ]\r\n        },\r\n        \"Entries\": null,\r\n        \"RelatedEntries\": null,\r\n        \"UserAccounts\": null,\r\n        \"Created\": \"2013-02-22T22:05:05.78Z\",\r\n        \"CreatedBy\": \"System\",\r\n        \"Modified\": \"2022-01-25T09:30:45.373Z\",\r\n        \"ModifiedBy\": \"madeleine.aviolat@bk.admin.ch\",\r\n        \"Id\": 90,\r\n        \"ConcurrencyToken\": 4517\r\n      }\r\n    ]\r\n  },\r\n  \"LenochClassifications\": null,\r\n  \"Glossaries\": {\r\n    \"$id\": \"41\",\r\n    \"$values\": [\r\n      {\r\n        \"$id\": \"42\",\r\n        \"Code\": \"G01\",\r\n        \"Translations\": {\r\n          \"$id\": \"43\",\r\n          \"$values\": []\r\n        },\r\n        \"Entries\": null,\r\n        \"UserAccounts\": null,\r\n        \"Created\": \"2023-04-05T10:15:07.574993Z\",\r\n        \"CreatedBy\": \"admin@termdat.ch\",\r\n        \"Modified\": \"2023-04-05T10:15:07.574889Z\",\r\n        \"ModifiedBy\": \"admin@termdat.ch\",\r\n        \"Id\": 1,\r\n        \"ConcurrencyToken\": 591\r\n      }\r\n    ]\r\n  },\r\n  \"Translations\": {\r\n    \"$id\": \"44\",\r\n    \"$values\": [\r\n      {\r\n        \"$id\": \"45\",\r\n        \"Sequence\": 1,\r\n        \"Terminus\": \"Terminus D\",\r\n        \"TerminusSource\": \"Quelle Terminus D\",\r\n        \"Name\": \"Name D\",\r\n        \"NameSource\": \"Quelle Name D\",\r\n        \"Abbreviation\": null,\r\n        \"AbbreviationSource\": null,\r\n        \"Phraseology\": null,\r\n        \"PhraseologySource\": null,\r\n        \"Definition\": null,\r\n        \"DefinitionSource\": null,\r\n        \"Note\": null,\r\n        \"NoteSource\": null,\r\n        \"Context\": null,\r\n        \"ContextSource\": null,\r\n        \"Country\": null,\r\n        \"Metadata\": null,\r\n        \"MetadataSource\": null,\r\n        \"IsObsolete\": false,\r\n        \"IsRare\": false,\r\n        \"ShouldAvoid\": false,\r\n        \"IsSynonym\": false,\r\n        \"LanguageId\": 2,\r\n        \"EntryId\": 0,\r\n        \"Language\": null,\r\n        \"Entry\": null,\r\n        \"ExternalSystemId\": null,\r\n        \"Id\": 1226013,\r\n        \"ConcurrencyToken\": 0\r\n      }\r\n    ]\r\n  },\r\n  \"ReferencedByEntries\": null,\r\n  \"References\": {\r\n    \"$id\": \"46\",\r\n    \"$values\": []\r\n  },\r\n  \"AdminInfos\": {\r\n    \"$id\": \"47\",\r\n    \"$values\": [\r\n      {\r\n        \"$id\": \"48\",\r\n        \"Code\": \"Brot\",\r\n        \"Description\": null,\r\n        \"Entries\": null,\r\n        \"Created\": \"2023-07-18T09:02:49.49975Z\",\r\n        \"CreatedBy\": \"admin@termdat.ch\",\r\n        \"Modified\": \"2023-07-18T09:02:49.499757Z\",\r\n        \"ModifiedBy\": \"admin@termdat.ch\",\r\n        \"Id\": 155,\r\n        \"ConcurrencyToken\": 4317\r\n      }\r\n    ]\r\n  },\r\n  \"ExternalSystemId\": null,\r\n  \"Created\": \"0001-01-01T00:00:00\",\r\n  \"CreatedBy\": null,\r\n  \"Modified\": \"0001-01-01T00:00:00\",\r\n  \"ModifiedBy\": null,\r\n  \"Id\": 0,\r\n  \"ConcurrencyToken\": 0\r\n}";
    }
}
