using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly DataContext context;

    public NewsRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<NewsItem> GetById(int id)
    {
        return await context.NewsItems
            .Include(ni => ni.Translations)
            .ThenInclude(t => t.Language)
            .SingleOrDefaultAsync(ni => ni.Id == id);
    }

    public async Task<NewsItem?> GetNextBySortIndex(int sortIndex)
    {
        return await context.NewsItems
            .Where(x => x.SortIndex > sortIndex)
            .OrderBy(x => x.SortIndex)
            .FirstOrDefaultAsync();
    }

    public async Task<NewsItem?> GetPreviousBySortIndex(int sortIndex)
    {
        return await context.NewsItems
            .Where(x => x.SortIndex < sortIndex)
            .OrderByDescending(x => x.SortIndex)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<NewsItem>> GetAll()
    {
        return await context.NewsItems
            .Include(ni => ni.Translations)
            .ThenInclude(t => t.Language)
            .OrderBy(ni => ni.SortIndex)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsItem>> GetAllActive()
    {
        return await context.NewsItems
            .Include(ni => ni.Translations)
            .ThenInclude(t => t.Language)
            .Where(y => y.IsEnabled && y.ValidFrom <= System.DateTime.UtcNow && y.ValidTo >= System.DateTime.UtcNow)
            .OrderBy(ni => ni.SortIndex)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Update(NewsItem existing, NewsItem update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);

        foreach (var updateTranslation in update.Translations)
        {
            var existingTranslation = existing.Translations
                .SingleOrDefault(t => t.LanguageId == updateTranslation.LanguageId);

            if (existingTranslation is not null)
            {
                existingTranslation.Text = updateTranslation.Text;
                existingTranslation.Title = updateTranslation.Title;
            }
            else
            {
                existing.Translations.Add(updateTranslation);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task Create(NewsItem newsItem)
    {
        await context.NewsItems.AddAsync(newsItem);
    }

    public void Delete(NewsItem newsItem)
    {
        context.NewsItems.Remove(newsItem);
    }

    public async Task<int> GetNextSortIndex()
    {
        var maxSortIndex = await context.NewsItems
            .MaxAsync(x => (int?)x.SortIndex) ?? 0;

        return maxSortIndex + 1;
    }
}
