using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Mapping.Profiles;

[TestFixture]
internal class SharedMappingProfileTests
{
    private IMapper mapper;

    private readonly ICultureService cultureServiceMock = Substitute.For<ICultureService>();

    [SetUp]
    public void Setup()
    {
        cultureServiceMock.ClearSubstitute();

        cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<DomainModels.IEntityNameTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<DomainModels.IEntityNameTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current")?.Name;
        });

        cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<DomainModels.NewsItemTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<DomainModels.NewsItemTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current");
        });

        cultureServiceMock.GetLanguageTranslation(Arg.Any<IEnumerable<DomainModels.LanguageTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<DomainModels.LanguageTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current")?.Name;
        });

        var services = new ServiceCollection();
        services.AddScoped(_ => cultureServiceMock);
        services.AddTransient(typeof(TranslationResolver<,>));
        services.AddTransient(typeof(LanguageTranslationResolver<,>));
        services.AddTransient(typeof(NewsItemTextResolver<,>));
        services.AddTransient(typeof(NewsItemTitleResolver<,>));
        var provider = services.BuildServiceProvider();

        using var loggerFactory = LoggerFactory.Create(_ => { });
        var config = new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new SharedMappingProfile());
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
    public void ShouldMapGlossarySummaryWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossary = new DomainModels.Glossary
        {
            Id = 1,
            Code = "g1",
            Translations = new List<DomainModels.GlossaryTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var glossarySummary = mapper.Map<ResourceModels.GlossarySummary>(glossary);


        Assert.That(glossarySummary.Id, Is.EqualTo(glossary.Id));
        Assert.That(glossarySummary.Code, Is.EqualTo(glossary.Code));
        Assert.That(glossarySummary.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ThrowWhenCollectionTranslationsLanguageIsNull()
    {
        var translations = new List<DomainModels.CollectionTranslation>()
        {
            new()
            {
                Name = "Translation",
                Language = null
            }
        };

        var collection = new DomainModels.Collection
        {
            Id = 1,
            Code = "Value",
            IsObsolete = false,
            Translations = translations
        };

        void Act() => mapper.Map<ResourceModels.CollectionSummary>(collection);

        var exception = Assert.Throws<AutoMapperMappingException>(Act);
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Cannot map translation without provided Language"));
    }

    [Test]
    public void ShouldMapOfficeWithCurrentLang()
    {
        var office = new DomainModels.Office
        {
            Id = 1,
            Code = "Value",
            Translations = new List<DomainModels.OfficeTranslation>()
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.OfficeSummary>(office);

        Assert.That(result.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ThrowWhenOfficeTranslationsLanguageIsNull()
    {
        var translations = new List<DomainModels.OfficeTranslation>()
        {
            new()
            {
                Name = "Translation",
                Language = null
            }
        };

        var office = new DomainModels.Office
        {
            Id = 1,
            Code = "Value",
            Translations = translations
        };

        void Act() => mapper.Map<ResourceModels.OfficeSummary>(office);

        var exception = Assert.Throws<AutoMapperMappingException>(Act);
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Cannot map translation without provided Language"));
    }

    [Test]
    public void ThrowWhenDomainTranslationsLanguageIsNull()
    {
        var translations = new List<DomainModels.LenochClassificationTranslation>()
        {
            new()
            {
                Name = "Translation",
                Language = null
            }
        };

        var domain = new DomainModels.LenochClassification
        {
            Id = 1,
            Code = "Value",
            Translations = translations
        };

        void Act() => mapper.Map<ResourceModels.LenochClassificationSummary>(domain);

        var exception = Assert.Throws<AutoMapperMappingException>(Act);
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Cannot map translation without provided Language"));
    }

    [Test]
    public void ShouldMapDomainWithCurrentLang()
    {
        var domain = new DomainModels.LenochClassification
        {
            Id = 1,
            Code = "Value",
            Translations = new List<DomainModels.LenochClassificationTranslation>()
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.LenochClassificationSummary>(domain);

        Assert.That(result.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ShouldMapConceptSystem()
    {
        var conceptSystem = new DomainModels.ConceptSystem
        {
            Id = 13,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch",
        };

        var result = mapper.Map<ResourceModels.ConceptSystemSummary>(conceptSystem);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(conceptSystem.Id));
    }

    [Test]
    public void ShouldMapOfficeWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var office = new DomainModels.Office
        {
            Id = 1,
            Code = "Value",
            Translations = new List<DomainModels.OfficeTranslation>()
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.OfficeSummary>(office);

        Assert.That(result.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ShouldMapSubjectSummaryAndDomainSummaryWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");

        var subject = new DomainModels.Subject
        {
            Id = 1,
            Translations = new List<DomainModels.SubjectTranslation>
            {
                new()
                {
                    Id = 1,
                    LanguageId = 1,
                    SubjectId = 1,
                    Name = "SUBJECT 1",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                },
                new()
                {
                    Id = 2,
                    LanguageId = 2,
                    SubjectId = 1,
                    Name = "SUBJECT 1 ALT",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
            },
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test@test.ch",
            Domains = new List<DomainModels.Domain>
            {
                new()
                {
                    Id = 1,
                    SubjectId = 1,
                    Created = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = "test@test.ch",
                    Modified = DateTime.UtcNow.AddDays(-5),
                    ModifiedBy = "test@test.ch",
                    Translations = new List<DomainModels.DomainTranslation>
                    {
                        new()
                        {
                            Id = 1,
                            LanguageId = 1,
                            DomainId = 1,
                            Name = "DOMAIN 1",
                            Language = new DomainModels.Language
                            {
                                IsoCode = "current"
                            }
                        },
                        new()
                        {
                            Id = 2,
                            LanguageId = 2,
                            DomainId = 1,
                            Name = "DOMAIN 1 ALT",
                            Language = new DomainModels.Language
                            {
                                IsoCode = "default"
                            }
                        },
                    }
                },
                new()
                {
                    Id = 2,
                    SubjectId = 1,
                    Created = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = "test@test.ch",
                    Modified = DateTime.UtcNow.AddDays(-5),
                    ModifiedBy = "test@test.ch",
                    Translations = new List<DomainModels.DomainTranslation>
                    {
                        new()
                        {
                            Id = 3,
                            LanguageId = 1,
                            DomainId = 2,
                            Name = "DOMAIN 2",
                            Language = new DomainModels.Language
                            {
                                IsoCode = "current"
                            }
                        },
                        new()
                        {
                            Id = 4,
                            LanguageId = 2,
                            DomainId = 2,
                            Name = "DOMAIN 2 ALT",
                            Language = new DomainModels.Language
                            {
                                IsoCode = "default"
                            }
                        },
                    }
                },
            }
        };
        var result = mapper.Map<ResourceModels.SubjectSummary>(subject);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("SUBJECT 1"));
        Assert.That(result.Domains, Is.Not.Empty);
        Assert.That(result.Domains[0].Id, Is.EqualTo(1));
        Assert.That(result.Domains[0].SubjectId, Is.EqualTo(1));
        Assert.That(result.Domains[0].Name, Is.EqualTo("DOMAIN 1"));
        Assert.That(result.Domains[1].Id, Is.EqualTo(2));
        Assert.That(result.Domains[1].SubjectId, Is.EqualTo(1));
        Assert.That(result.Domains[1].Name, Is.EqualTo("DOMAIN 2"));
    }

    [Test]
    public void ShouldMapLanguageDetailWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");

        var language = new DomainModels.Language
        {
            Id = 1,
            IsoCode = "Value",
            SortIndex = 33,
            Created = new DateTime(2022, 1, 1, 1, 1, 1),
            Modified = new DateTime(2022, 1, 1, 1, 1, 1),
            CreatedBy = "CreatedBy",
            ModifiedBy = "ModifiedBy",
            Translations = new List<DomainModels.LanguageTranslation>()
            {
                new()
                {
                    Name = "Default Translation",
                    IsoCode = "default",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "default"
                    }
                },
                new()
                {
                    Name = "Current Translation",
                    IsoCode = "current",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.LanguageDetail>(language);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(language.Id));
        Assert.That(result.IsoCode, Is.EqualTo(language.IsoCode));
        Assert.That(result.Created, Is.EqualTo(language.Created));
        Assert.That(result.Modified, Is.EqualTo(language.Modified));
        Assert.That(result.CreatedBy, Is.EqualTo(language.CreatedBy));
        Assert.That(result.ModifiedBy, Is.EqualTo(language.ModifiedBy));
        Assert.That(result.Name, Is.EqualTo(language.Translations.Last().Name));
    }

    [Test]
    public void ThrowsOnLanguageDetailWhenLanguageIsNull()
    {
        var translations = new List<DomainModels.LanguageTranslation>()
        {
            new()
            {
                Name = "Translation",
                Language = null
            }
        };

        var language = new DomainModels.Language
        {
            Id = 1,
            IsoCode = "de",
            Translations = translations
        };

        void Act() => mapper.Map<ResourceModels.LanguageDetail>(language);

        var expectedMessage = "Cannot map translation without provided Language";

        var exception = Assert.Throws<AutoMapperMappingException>(Act, expectedMessage);

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void ShouldMapAdminInfoSummary()
    {
        var adminInfo = new DomainModels.AdminInfo
        {
            Id = 13,
            Code = "Test",
            Description = "Description",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch",
        };

        var result = mapper.Map<ResourceModels.AdminInfoSummary>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(adminInfo.Id));
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_NewsItemToNewsItemSummary_ShouldMap()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 1,
            CreatedBy = "creator",
            ModifiedBy = "modifier",
            Created = DateTime.Now,
            Modified = DateTime.Now,
            Translations = new List<DomainModels.NewsItemTranslation>
            {
                new()
                {
                    Title = "German title",
                    Text = "German text",
                    Language = new DomainModels.Language
                    {
                        IsoCode = "current"
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.NewsItemSummary>(newsItem);

        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("German title"));
        Assert.That(result.Text, Is.EqualTo("German text"));
        Assert.That(result.CreatedBy, Is.EqualTo("creator"));
    }

    [Test]
    public void Map_WhenTranslationLanguageIsNull_ShouldThrow()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Translations = new List<DomainModels.NewsItemTranslation>
            {
                new()
                {
                    Title = "Title",
                    Text = "Text",
                    Language = null
                }
            }
        };

        Assert.Throws<AutoMapperMappingException>(() => mapper.Map<ResourceModels.NewsItemSummary>(newsItem));
    }
}
