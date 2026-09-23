using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class NewsServiceTests
{
    private NewsService newsService;
    private readonly INewsRepository newsRepositoryMock = Substitute.For<INewsRepository>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private IEnumerable<DomainModels.NewsItem> newsItems;
    private NewsItemSummary newsItemSummary;
    private NewsItemUpdate newsItemUpdate;

    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly ILanguageNewsTranslationMapper languageNewsTranslationMapperMock = Substitute.For<ILanguageNewsTranslationMapper>();

    [SetUp]
    public void SetUp()
    {
        newsService = new NewsService(
            mapperMock,
            newsRepositoryMock,
            identityServiceMock,
            unitOfWorkMock,
            languageNewsTranslationMapperMock
        );
        identityServiceMock.GetCurrentEmail().Returns("writer@admin.ch");

        newsItems = new List<DomainModels.NewsItem>
        {
            new()
            {
                Id = 1,                
            }
        };

        newsItemSummary = new NewsItemSummary
        {
            Id = 1
        };

        newsItemUpdate = new NewsItemUpdate
        {
            Id = 1,
            SortIndex = 5,
            IsEnabled = true,
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2025, 2, 2),
            TitleDe = "Title DE",
            TitleFr = "Title FR",
            TitleIt = "Title IT",
            TitleRm = "Title RM",
            TitleEn = "Title EN",
            TextDe = "Text DE",
            TextFr = "Text FR",
            TextIt = "Text IT",
            TextRm = "Text RM",
            TextEn = "Text EN"
        };

        newsRepositoryMock.GetAll().Returns(newsItems);
        newsRepositoryMock.GetAllActive().Returns(newsItems);
        mapperMock.Map<IEnumerable<NewsItemSummary>>(newsItems)
            .Returns(new List<NewsItemSummary> { newsItemSummary });
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        newsRepositoryMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        languageNewsTranslationMapperMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAll_WhenCalled_ShouldReturnNewsSummaries()
    {
        var result = await newsService.GetAll();

        Assert.That(result, Is.EqualTo(new List<NewsItemSummary> { newsItemSummary }));

        await newsRepositoryMock.Received(1).GetAll();
    }

    [Test]
    public async Task GetAllActive_WhenCalled_ShouldReturnNewsSummaries()
    {
        var result = await newsService.GetAllActive();

        Assert.That(result, Is.EqualTo(new List<NewsItemSummary> { newsItemSummary }));

        await newsRepositoryMock.Received(1).GetAllActive();
    }

    [Test]
    public async Task GetDetails_WhenCalled_ShouldReturnNewsUpdate()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 1,
            Translations = new List<DomainModels.NewsItemTranslation>
            {
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "de"
                    },
                    Title = "Title DE",
                    Text = "Text DE"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "fr"
                    },
                    Title = "Title FR",
                    Text = "Text FR"
                }
            }
        };

        newsRepositoryMock.GetById(1).Returns(newsItem);
        mapperMock.Map<NewsItemUpdate>(newsItem).Returns(newsItemUpdate);

        var result = await newsService.GetDetails(1);

        Assert.That(result, Is.EqualTo(newsItemUpdate));

        Assert.That(result.TitleDe, Is.EqualTo("Title DE"));
        Assert.That(result.TextDe, Is.EqualTo("Text DE"));

        Assert.That(result.TitleFr, Is.EqualTo("Title FR"));
        Assert.That(result.TextFr, Is.EqualTo("Text FR"));
    }

    [Test]
    public async Task GetDetails_WhenCalledWithInvalidId_ShouldThrowEntityNotFoundException()
    {
        newsRepositoryMock.GetById(0)
            .Returns(Task.FromResult<DomainModels.NewsItem>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await newsService.GetDetails(0)
        );

        await newsRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task Create_WhenCalled_ShouldCreateNewsAndReturnDetail()
    {
        var newsItemCreate = new NewsItemCreate()
        {
            TitleDe = "Title DE",
            TitleFr = "Title FR",
            TitleIt = "Title IT",
            TitleRm = "Title RM",
            TitleEn = "Title EN",
            TextDe = "Text DE",
            TextFr = "Text FR",
            TextIt = "Text IT",
            TextRm = "Text RM",
            TextEn = "Text EN",
            IsEnabled = true,
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2025, 1, 3)
        };

        var mappedNewsItem = new DomainModels.NewsItem()
        {
            Id = 1,            
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2025, 1, 3),
            IsEnabled = true,
            Translations = [
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "de"
                    },
                    Title = "Title DE",
                    Text = "Text DE"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "fr"
                    },
                    Title = "Title FR",
                    Text = "Text FR"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "it"
                    },
                    Title = "Title IT",
                    Text = "Text IT"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "em"
                    },
                    Title = "Title RM",
                    Text = "Text RM"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "en"
                    },
                    Title = "Title EN",
                    Text = "Text EN"
                }]
        };
        var expectedDetail = new NewsItemDetail();

        mapperMock.Map<DomainModels.NewsItem>(newsItemCreate).Returns(mappedNewsItem);
        languageNewsTranslationMapperMock.Map(newsItemCreate)
            .Returns(mappedNewsItem.Translations);

        newsRepositoryMock.GetNextSortIndex().Returns(5);

        mapperMock.Map<NewsItemDetail>(mappedNewsItem).Returns(expectedDetail);

        var result = await newsService.Create(newsItemCreate);

        Assert.That(result, Is.EqualTo(expectedDetail));

        Assert.That(mappedNewsItem.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-1)));
        Assert.That(mappedNewsItem.ModifiedBy, Is.EqualTo("writer@admin.ch"));
        Assert.That(mappedNewsItem.Created, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-1)));
        Assert.That(mappedNewsItem.CreatedBy, Is.EqualTo("writer@admin.ch"));
        Assert.That(mappedNewsItem.SortIndex, Is.EqualTo(5));
        Assert.That(mappedNewsItem.ValidFrom, Is.EqualTo(newsItemCreate.ValidFrom));
        Assert.That(mappedNewsItem.ValidTo, Is.EqualTo(newsItemCreate.ValidTo));
        Assert.That(mappedNewsItem.IsEnabled, Is.EqualTo(newsItemCreate.IsEnabled));
        Assert.That(mappedNewsItem.Translations, Has.Count.EqualTo(5));

        await newsRepositoryMock.Received(1).Create(mappedNewsItem);
        await languageNewsTranslationMapperMock.Received(1).Map(Arg.Any<INewsItemModification>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Delete_WhenCalledWithInvalidId_ShouldThrowEntityNotFoundException()
    {
        newsRepositoryMock.GetById(1)
            .Returns(Task.FromResult<DomainModels.NewsItem>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await newsService.Delete(1));

        await newsRepositoryMock.Received(1).GetById(1);
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateNewsAndReturnDetail()
    {
        var createdDate = DateTime.UtcNow.AddDays(-1);

        var existingNews = new DomainModels.NewsItem
        {
            Id = 1,
            Created = createdDate,
            CreatedBy = "creator@test.ch",
            Translations = [
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "de"
                    },
                    Title = "Title DE old",
                    Text = "Text DE old"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "fr"
                    },
                    Title = "Title FR old",
                    Text = "Text FR old"
                }]
        };

        var expectedDetail = new NewsItemDetail();

        newsRepositoryMock.GetById(1).Returns(existingNews);

        languageNewsTranslationMapperMock.Map(newsItemUpdate)
            .Returns(new List<DomainModels.NewsItemTranslation>()
            {
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "de"
                    },
                    Title = "Title DE",
                    Text = "Text DE"
                },
                new()
                {
                    Language = new DomainModels.Language
                    {
                        IsoCode = "fr"
                    },
                    Title = "Title FR",
                    Text = "Text FR"
                }
            });

        mapperMock.Map<NewsItemDetail>(existingNews).Returns(expectedDetail);

        var result = await newsService.Update(newsItemUpdate);

        Assert.That(result, Is.EqualTo(expectedDetail));

        Assert.That(existingNews.Created, Is.EqualTo(createdDate));
        Assert.That(existingNews.CreatedBy, Is.EqualTo("creator@test.ch"));
        Assert.That(existingNews.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-1)));
        Assert.That(existingNews.ModifiedBy, Is.EqualTo("writer@admin.ch"));
        Assert.That(existingNews.ValidFrom, Is.EqualTo(newsItemUpdate.ValidFrom));
        Assert.That(existingNews.ValidTo, Is.EqualTo(newsItemUpdate.ValidTo));
        Assert.That(existingNews.SortIndex, Is.EqualTo(newsItemUpdate.SortIndex));
        Assert.That(existingNews.IsEnabled, Is.EqualTo(newsItemUpdate.IsEnabled));
        Assert.That(existingNews.Translations, Has.Count.EqualTo(2));
        Assert.That(existingNews.Translations.First().Title, Is.EqualTo("Title DE"));
        Assert.That(existingNews.Translations.Last().Title, Is.EqualTo("Title FR"));
        Assert.That(existingNews.Translations.First().Text, Is.EqualTo("Text DE"));
        Assert.That(existingNews.Translations.Last().Text, Is.EqualTo("Text FR"));

        await languageNewsTranslationMapperMock.Received(1).Map(Arg.Any<INewsItemModification>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task MoveItemUp_WhenPreviousItemExists_ShouldSwapSortIndex()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 2,
            SortIndex = 2
        };

        var previousItem = new DomainModels.NewsItem
        {
            Id = 1,
            SortIndex = 1
        };

        newsRepositoryMock.GetById(2).Returns(newsItem);
        newsRepositoryMock.GetPreviousBySortIndex(2).Returns(previousItem);

        await newsService.MoveItemUp(2);

        Assert.That(newsItem.SortIndex, Is.EqualTo(1));
        Assert.That(previousItem.SortIndex, Is.EqualTo(2));

        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task MoveItemUp_WhenAlreadyAtTop_ShouldThrowBusinessValidationException()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 1,
            SortIndex = 1
        };

        newsRepositoryMock.GetById(1).Returns(newsItem);
        newsRepositoryMock.GetPreviousBySortIndex(1).Returns((DomainModels.NewsItem)null);

        var exception = Assert.ThrowsAsync<BusinessValidationException>(
            async () => await newsService.MoveItemUp(1)
        );

        Assert.That(exception.Message, Does.Contain("already at the top"));
    }

    [Test]
    public async Task MoveItemDown_WhenNextItemExists_ShouldSwapSortIndex()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 1,
            SortIndex = 1
        };

        var nextItem = new DomainModels.NewsItem
        {
            Id = 2,
            SortIndex = 2
        };

        newsRepositoryMock.GetById(1).Returns(newsItem);
        newsRepositoryMock.GetNextBySortIndex(1).Returns(nextItem);

        await newsService.MoveItemDown(1);

        Assert.That(newsItem.SortIndex, Is.EqualTo(2));
        Assert.That(nextItem.SortIndex, Is.EqualTo(1));

        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task MoveItemDown_WhenAlreadyAtBottom_ShouldThrowBusinessValidationException()
    {
        var newsItem = new DomainModels.NewsItem
        {
            Id = 2,
            SortIndex = 2
        };

        newsRepositoryMock.GetById(2).Returns(newsItem);
        newsRepositoryMock.GetNextBySortIndex(2).Returns((DomainModels.NewsItem)null);

        var exception = Assert.ThrowsAsync<BusinessValidationException>(
            () => newsService.MoveItemDown(2)
        );

        Assert.That(exception.Message, Does.Contain("already at the bottom"));
    }
}
