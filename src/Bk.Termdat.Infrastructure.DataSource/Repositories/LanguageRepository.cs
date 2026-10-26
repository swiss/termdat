using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly DataContext context;

    public LanguageRepository(DataContext dataContext)
    {
        context = dataContext;
    }

    public async Task<IEnumerable<LanguageTranslation>> GetTranslationByIsoCode(string isoCode)
    {
        return await context.LanguageTranslations
            .Include(l => l.Language)
            .Where(l => l.IsoCode.ToUpper().Equals(isoCode.ToUpper()))
            .OrderBy(l => l.Language.SortIndex)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Language>> GetAll()
    {
        return await context.Languages
            .OrderBy(l => l.SortIndex)
            .ToListAsync();
    }

    public async Task<IEnumerable<Language>> GetAllForValidation()
    {
        return await context.Languages
            .OrderBy(l => l.SortIndex)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Language>> GetAllDetails()
    {
        return await context.Languages
            .Include(l => l.Translations)
            .OrderBy(l => l.SortIndex)
            .ToListAsync();
    }

    public async Task<Language> GetById(int id)
    {
        return await context.Languages
            .Include(e => e.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Create(Language language)
    {
        await context.Languages.AddAsync(language);
    }

    public void Update(Language existing, Language update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        context.Entry(existing).CurrentValues.SetValues(update);

        foreach (var updateTranslation in update.Translations)
        {
            var existingTranslations = existing.Translations.Where(t => t.LanguageId == updateTranslation.LanguageId && t.IsoCode == updateTranslation.IsoCode).ToList();
            if (existingTranslations.Any())
            {
                if (existingTranslations.Count != 1)
                {
                    throw new InvalidOperationException($"Language with id='{existing.Id}' has multiple translations for language id='{updateTranslation.LanguageId}' and isoCode='{updateTranslation.IsoCode}'.");
                }

                var existingTranslation = existingTranslations.Single();
                existingTranslation.Name = updateTranslation.Name;
            }
            else
            {
                existing.Translations.Add(updateTranslation);
            }
        }
    }

    public int GetNextSort()
    {
        var max = context.Languages.Max(p => (int?)p.SortIndex) ?? 0;
        return max + 1;
    }
}

