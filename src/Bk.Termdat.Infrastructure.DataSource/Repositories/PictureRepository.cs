using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class PictureRepository : IPictureRepository
{
    private readonly DataContext context;

    public PictureRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<Picture> GetById(int entryId)
    {
        return await context.Pictures
            .Include(p => p.Translations)
            .Include(p => p.File)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EntryId == entryId);
    }

    public async Task<Picture> GetByIdForUpdate(int entryId)
    {
        return await context.Pictures
            .Include(p => p.Translations)
            .Include(p => p.File)
            .FirstOrDefaultAsync(p => p.EntryId == entryId);
    }

    public async Task<Picture> Create(Picture picture)
    {
        await context.Pictures.AddAsync(picture);

        return picture;
    }

    public async Task Delete(int entryId)
    {
        var picture = await context.Pictures.FirstOrDefaultAsync(p => p.EntryId == entryId);
        if (picture != null)
        {
            context.Pictures.Remove(picture);
        }
    }

    public void Update(Picture existing, Picture update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        context.Entry(existing.File).CurrentValues.SetValues(update.File);

        // Translations could have been removed for some languages
        var translationsToDelete = existing.Translations.Where(t => update.Translations.All(t2 => t2.LanguageId != t.LanguageId)).ToList();

        // Create new translations or update existing
        foreach (var pictureTranslation in update.Translations)
        {
            var existingTranslation = existing.Translations.FirstOrDefault(t => t.LanguageId == pictureTranslation.LanguageId);
            if (existingTranslation == null)
            {
                existing.Translations.Add(pictureTranslation);
            }
            else
            {
                existingTranslation.Description = pictureTranslation.Description;
                existingTranslation.Source = pictureTranslation.Source;
            }
        }

        context.PictureTranslations.RemoveRange(translationsToDelete);
    }
}
