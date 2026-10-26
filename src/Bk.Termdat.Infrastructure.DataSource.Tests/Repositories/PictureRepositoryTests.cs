using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class PictureRepositoryTests
{
    private PictureRepository pictureRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        pictureRepository = new PictureRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetByIdReturnsPictureWithTranslationsAndFile()
    {
        var picture = await pictureRepository.GetById(1);

        Assert.That(picture, Is.Not.Null);
        Assert.That(picture.File, Is.Not.Null);
        Assert.That(picture.Translations, Is.Not.Null);
        Assert.That(picture.Translations.Count, Is.EqualTo(2));

        Assert.That(picture.EntryId, Is.EqualTo(1));
        Assert.That(picture.ContentType, Is.EqualTo("image/png"));
        Assert.That(picture.FileExtension, Is.EqualTo("png"));
        Assert.That(picture.File.EntryId, Is.EqualTo(1));
        Assert.That(picture.File.Value, Is.EqualTo(Encoding.ASCII.GetBytes("TEST IMAGE")));

        var translations = picture.Translations.ToList();
        Assert.That(translations[0].EntryId, Is.EqualTo(1));
        Assert.That(translations[0].Description, Is.EqualTo("Description Deutsch"));
        Assert.That(translations[0].Source, Is.EqualTo("Source Deutsch"));
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));

        Assert.That(translations[1].EntryId, Is.EqualTo(1));
        Assert.That(translations[1].Description, Is.EqualTo("Description Französisch"));
        Assert.That(translations[1].Source, Is.EqualTo("Source Französisch"));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdForUpdateReturnsPictureWithTranslationsAndFile()
    {
        var picture = await pictureRepository.GetByIdForUpdate(1);

        Assert.That(picture, Is.Not.Null);
        Assert.That(picture.File, Is.Not.Null);
        Assert.That(picture.Translations, Is.Not.Null);
        Assert.That(picture.Translations.Count, Is.EqualTo(2));

        Assert.That(picture.EntryId, Is.EqualTo(1));
        Assert.That(picture.ContentType, Is.EqualTo("image/png"));
        Assert.That(picture.FileExtension, Is.EqualTo("png"));
        Assert.That(picture.File.EntryId, Is.EqualTo(1));
        Assert.That(picture.File.Value, Is.EqualTo(Encoding.ASCII.GetBytes("TEST IMAGE")));

        var translations = picture.Translations.ToList();
        Assert.That(translations[0].EntryId, Is.EqualTo(1));
        Assert.That(translations[0].Description, Is.EqualTo("Description Deutsch"));
        Assert.That(translations[0].Source, Is.EqualTo("Source Deutsch"));
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));

        Assert.That(translations[1].EntryId, Is.EqualTo(1));
        Assert.That(translations[1].Description, Is.EqualTo("Description Französisch"));
        Assert.That(translations[1].Source, Is.EqualTo("Source Französisch"));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));
    }

    [Test]
    public async Task CanCreate()
    {
        await pictureRepository.Create(new Picture()
        {
            EntryId = 100,
            ContentType = "test",
            FileExtension = "test",
            EntityTag = "tag"
        });

        await context.SaveChangesAsync();

        var saved = await context.Pictures.SingleAsync(p => p.EntryId == 100);

        Assert.That(saved.EntryId, Is.EqualTo(100));
        Assert.That(saved.ContentType, Is.EqualTo("test"));
        Assert.That(saved.FileExtension, Is.EqualTo("test"));
    }

    [Test]
    public async Task CanDelete()
    {
        Assert.DoesNotThrowAsync(async () =>
        {
            await pictureRepository.Delete(200);
            await context.SaveChangesAsync();
        });

        await pictureRepository.Create(new Picture()
        {
            EntryId = 200,
            ContentType = "test",
            FileExtension = "test",
            EntityTag = "tag"
        });

        await context.SaveChangesAsync();

        await pictureRepository.Delete(200);
        await context.SaveChangesAsync();

        var deleted = await context.Pictures.FirstOrDefaultAsync(p => p.EntryId == 200);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task CanUpdate()
    {
        var existing = await pictureRepository.GetByIdForUpdate(2);
        var updatedPicture = await pictureRepository.GetById(2);

        updatedPicture.File.Value = Encoding.ASCII.GetBytes("NEW TEST IMAGE");
        updatedPicture.ContentType = "image/gif";
        updatedPicture.FileExtension = ".gif";
        updatedPicture.Translations.First().Description = "First Description";
        updatedPicture.Translations.First().Source = null;
        updatedPicture.Translations.Add(new PictureTranslation() { EntryId = updatedPicture.EntryId, LanguageId = 4, Description = "New Description", Source = "New Source" });

        pictureRepository.Update(existing, updatedPicture);

        await context.SaveChangesAsync();

        var afterSave = await pictureRepository.GetById(2);

        Assert.That(afterSave, Is.Not.Null);
        Assert.That(afterSave.File, Is.Not.Null);
        Assert.That(afterSave.Translations, Is.Not.Null);
        Assert.That(afterSave.Translations.Count, Is.EqualTo(3));

        Assert.That(afterSave.EntryId, Is.EqualTo(2));
        Assert.That(afterSave.ContentType, Is.EqualTo("image/gif"));
        Assert.That(afterSave.FileExtension, Is.EqualTo(".gif"));
        Assert.That(afterSave.File.EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(afterSave.File.Value, Is.EqualTo(Encoding.ASCII.GetBytes("NEW TEST IMAGE")));

        var translations = afterSave.Translations.ToList();
        Assert.That(translations[0].EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(translations[0].Description, Is.EqualTo("First Description"));
        Assert.That(translations[0].Source, Is.Null);
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));

        Assert.That(translations[1].EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(translations[1].Description, Is.EqualTo("Description Französisch 2"));
        Assert.That(translations[1].Source, Is.EqualTo("Source Französisch 2"));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));

        Assert.That(translations[2].EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(translations[2].Description, Is.EqualTo("New Description"));
        Assert.That(translations[2].Source, Is.EqualTo("New Source"));
        Assert.That(translations[2].LanguageId, Is.EqualTo(4));
    }

    [Test]
    public async Task UpdateCanAddUpdateAndDeletePictureTranslations()
    {
        var existing = await pictureRepository.GetByIdForUpdate(2);
        var updatedPicture = await pictureRepository.GetById(2);

        updatedPicture.Translations.Remove(updatedPicture.Translations.First());
        updatedPicture.Translations.Add(new PictureTranslation() { EntryId = updatedPicture.EntryId, LanguageId = 4, Description = "New Description", Source = "New Source" });

        pictureRepository.Update(existing, updatedPicture);

        await context.SaveChangesAsync();

        var afterSave = await pictureRepository.GetById(2);

        Assert.That(afterSave, Is.Not.Null);
        Assert.That(afterSave.File, Is.Not.Null);
        Assert.That(afterSave.Translations, Is.Not.Null);
        Assert.That(afterSave.Translations.Count, Is.EqualTo(2));

        var translations = afterSave.Translations.ToList();
        Assert.That(translations[0].EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(translations[0].Description, Is.EqualTo("Description Französisch 2"));
        Assert.That(translations[0].Source, Is.EqualTo("Source Französisch 2"));
        Assert.That(translations[0].LanguageId, Is.EqualTo(2));

        Assert.That(translations[1].EntryId, Is.EqualTo(afterSave.EntryId));
        Assert.That(translations[1].Description, Is.EqualTo("New Description"));
        Assert.That(translations[1].Source, Is.EqualTo("New Source"));
        Assert.That(translations[1].LanguageId, Is.EqualTo(4));
    }
}
