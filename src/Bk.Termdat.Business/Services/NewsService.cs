using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Services;

public class NewsService : INewsService
{
    private readonly IMapper mapper;
    private readonly INewsRepository newsRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IIdentityService identityService;
    private readonly ILanguageNewsTranslationMapper languageNewsTranslationMapper;

    public NewsService(
        IMapper mapper,
        INewsRepository newsRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILanguageNewsTranslationMapper languageNewsTranslationMapper)
    {
        this.mapper = mapper;
        this.newsRepository = newsRepository;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.languageNewsTranslationMapper = languageNewsTranslationMapper;
    }

    public async Task<IEnumerable<NewsItemSummary>> GetAll()
    {
        var newsItems = await newsRepository.GetAll();
        var mappedItems = mapper.Map<IEnumerable<NewsItemSummary>>(newsItems).ToList();

        for (var i = 0; i < mappedItems.Count; i++)
        {
            mappedItems[i].CanMoveUp = i > 0;
            mappedItems[i].CanMoveDown = i < mappedItems.Count - 1;
        }

        return mappedItems;
    }

    public async Task<IEnumerable<NewsItemSummary>> GetAllActive()
    {
        var newsItems = await newsRepository.GetAllActive();
        return mapper.Map<IEnumerable<NewsItemSummary>>(newsItems);
    }

    public async Task<IEnumerable<NewsItemDetail>> GetAllDetails()
    {
        var newsItems = await newsRepository.GetAll();
        return mapper.Map<IEnumerable<NewsItemDetail>>(newsItems);
    }

    public async Task<NewsItemUpdate> GetDetails(int id)
    {
        var newsItem = await newsRepository.GetById(id);
        if (newsItem is null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.NewsNotFound, id));
        }

        var dest = mapper.Map<NewsItemUpdate>(newsItem);

        languageNewsTranslationMapper.MapTranslationsTo(newsItem.Translations, dest);

        return dest;
    }

    public async Task<NewsItemDetail> Create(NewsItemCreate newsItemCreate)
    {
        var mappedNewsItem = mapper.Map<NewsItem>(newsItemCreate);
        mappedNewsItem.Created = DateTime.UtcNow;
        mappedNewsItem.CreatedBy = identityService.GetCurrentEmail();
        mappedNewsItem.Modified = DateTime.UtcNow;
        mappedNewsItem.ModifiedBy = identityService.GetCurrentEmail();

        mappedNewsItem.SortIndex = await newsRepository.GetNextSortIndex();

        mappedNewsItem.Translations = (await languageNewsTranslationMapper.Map(newsItemCreate)).ToList();

        await newsRepository.Create(mappedNewsItem);

        await unitOfWork.SaveChangesAsync();

        return mapper.Map<NewsItemDetail>(mappedNewsItem);
    }

    public async Task Delete(int id)
    {
        var newsItem = await newsRepository.GetById(id);

        if (newsItem == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.NewsNotFound, id));
        }

        newsRepository.Delete(newsItem);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<NewsItemDetail> Update(NewsItemUpdate newsItemUpdate)
    {
        ArgumentNullException.ThrowIfNull(newsItemUpdate);

        var databaseNewsItem = await newsRepository.GetById(newsItemUpdate.Id);

        if (databaseNewsItem == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.NewsNotFound, newsItemUpdate.Id));
        }

        databaseNewsItem.SortIndex = newsItemUpdate.SortIndex;
        databaseNewsItem.IsEnabled = newsItemUpdate.IsEnabled;
        databaseNewsItem.ValidFrom = newsItemUpdate.ValidFrom;
        databaseNewsItem.ValidTo = newsItemUpdate.ValidTo;
        databaseNewsItem.Modified = DateTime.UtcNow;
        databaseNewsItem.ModifiedBy = identityService.GetCurrentEmail();
        databaseNewsItem.Translations = (await languageNewsTranslationMapper.Map(newsItemUpdate)).ToList();

        await unitOfWork.SaveChangesAsync();

        return mapper.Map<NewsItemDetail>(databaseNewsItem);
    }

    public async Task MoveItemUp(int id)
    {
        var databaseNewsItem = await GetNewsItem(id);

        var itemToSwap = await newsRepository.GetPreviousBySortIndex(databaseNewsItem.SortIndex);

        if (itemToSwap == null)
        {
            throw new BusinessValidationException(nameof(NewsItem.SortIndex),
                $"News item with id {id} is already at the top");
        }        

        await SwapSortIndex(databaseNewsItem, itemToSwap);
    }

    public async Task MoveItemDown(int id)
    {
        var databaseNewsItem = await GetNewsItem(id);

        var itemToSwap = await newsRepository.GetNextBySortIndex(databaseNewsItem.SortIndex);

        if (itemToSwap == null)
        {
            throw new BusinessValidationException(nameof(NewsItem.SortIndex),
                $"News item with id {id} is already at the bottom");
        }

        await SwapSortIndex(databaseNewsItem, itemToSwap);
    }

    private async Task<NewsItem> GetNewsItem(int id)
    {
        var newsItem = await newsRepository.GetById(id);

        if (newsItem == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.NewsNotFound, id));
        }

        return newsItem;
    }

    private async Task SwapSortIndex(NewsItem first, NewsItem second)
    {
        var tempSortIndex = first.SortIndex;
        first.SortIndex = second.SortIndex;
        second.SortIndex = tempSortIndex;

        var now = DateTime.UtcNow;
        var user = identityService.GetCurrentEmail();

        first.Modified = now;
        first.ModifiedBy = user;

        second.Modified = now;
        second.ModifiedBy = user;

        await unitOfWork.SaveChangesAsync();
    }
}
