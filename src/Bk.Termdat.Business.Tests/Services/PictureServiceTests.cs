using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using SkiaSharp;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
[SetUICulture("de")]
internal class PictureServiceTests
{
    private IPictureRepository pictureRepositoryMock;
    private ILanguageRepository languageRepositoryMock;
    private Picture picture;
    private PictureService pictureService;
    private List<Language> languages;
    private PictureDetail dto;
    private FormFile actualFile;
    private readonly IPictureTranslationMapper pictureTranslationMapper = new PictureTranslationMapper();

    [SetUp]
    public void SetUp()
    {
        picture = new()
        {
            EntryId = 1,
            ContentType = "image/png",
            FileExtension = "png",
            File = new PictureFile
            {
                EntryId = 1,
                Value = Encoding.ASCII.GetBytes("TEST IMAGE")
            },
            Translations = new List<PictureTranslation>
            {
                new() { Id = 1, EntryId = 1, LanguageId = 1, Description = "TestDescription", Source = "Source" }
            }
        };
        languages = new List<Language>
        {
            new() { Id = 1, IsoCode = "DE" },
            new() { Id = 2, IsoCode = "EN" },
            new() { Id = 3, IsoCode = "FR" },
            new() { Id = 4, IsoCode = "IT" },
            new() { Id = 5, IsoCode = "RM" },
            new() { Id = 6, IsoCode = "ES" },
        };
        dto = new PictureDetail
        {
            EntryId = 1,
            DescriptionDe = "DescriptionDe",
            SourceDe = "SourceDe",
            DescriptionIt = "DescriptionIt",
            SourceEn = "SourceEn",
        };
        pictureRepositoryMock = Substitute.For<IPictureRepository>();
        languageRepositoryMock = Substitute.For<ILanguageRepository>();
        actualFile = GetFormFile();

        pictureRepositoryMock.GetById(Arg.Any<int>()).Returns(picture);
        languageRepositoryMock.GetAll().Returns(languages);
        pictureRepositoryMock.GetByIdForUpdate(Arg.Is(1)).Returns(picture);

        pictureService = new PictureService(pictureRepositoryMock, languageRepositoryMock, pictureTranslationMapper);
    }

    [Test]
    public async Task GetPictureReturnsCorrectFileResult()
    {
        var result = await pictureService.GetPicture(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Picture>());
        Assert.That(result.ContentType, Is.EqualTo(picture.ContentType));
        Assert.That(result.File.Value, Is.EqualTo(picture.File.Value));

        await pictureRepositoryMock.Received(1).GetById(Arg.Is(1));
    }

    [Test]
    public async Task GetPictureCanHandleNullValues()
    {
        pictureRepositoryMock.GetById(Arg.Any<int>()).ReturnsNull();

        var pictureFile = await pictureService.GetPicture(1);
        Assert.That(pictureFile, Is.Null);

        picture.File = null;
        pictureRepositoryMock.GetById(Arg.Any<int>()).ReturnsForAnyArgs(picture);
        var pictureFile2 = await pictureService.GetPicture(1);
        Assert.That(pictureFile2, Is.Null);
    }

    [Test]
    public async Task CanDelete()
    {
        await pictureService.Delete(1);

        await pictureRepositoryMock.Received(1).Delete(1);
    }

    [Test]
    public async Task CanCreate()
    {
        await using var ms = new MemoryStream();
        await actualFile.CopyToAsync(ms);
        const string expectedETag = "A06D702FCBC38B36624E67E852E646CE7F1E96DA";
        var toHash = "       "u8.ToArray();

        PictureEntityTagService.ComputeEntityTag(toHash);

        var created = await pictureService.Create(dto, actualFile);

        Assert.That(created.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(created.EntityTag, Is.EqualTo(expectedETag));
        Assert.That(created.ContentType, Is.EqualTo("image/jpeg"));
        Assert.That(created.FileExtension, Is.EqualTo(".jpg"));
        Assert.That(created.File, Is.Not.Null);
        Assert.That(created.File.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(created.File.Value, Is.EqualTo(ms.ToArray()));

        var translations = created.Translations.ToList();
        Assert.That(translations.Count, Is.EqualTo(3));

        Assert.That(translations[0].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));
        Assert.That(translations[0].Description, Is.EqualTo("DescriptionDe"));
        Assert.That(translations[0].Source, Is.EqualTo("SourceDe"));

        Assert.That(translations[1].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));
        Assert.That(translations[1].Description, Is.Null);
        Assert.That(translations[1].Source, Is.EqualTo("SourceEn"));

        Assert.That(translations[2].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[2].LanguageId, Is.EqualTo(4));
        Assert.That(translations[2].Description, Is.EqualTo("DescriptionIt"));
        Assert.That(translations[2].Source, Is.Null);

        await pictureRepositoryMock.Received(1).Create(created);
    }

    [Test]
    public async Task CanUpdate()
    {
        await using var ms = new MemoryStream();
        await actualFile.CopyToAsync(ms);
        const string expectedETag = "A06D702FCBC38B36624E67E852E646CE7F1E96DA";
        var toHash = "       "u8.ToArray();

        PictureEntityTagService.ComputeEntityTag(toHash);
        Picture pictureToUpdate = null;
        pictureRepositoryMock.Update(Arg.Any<Picture>(), Arg.Do<Picture>(x => pictureToUpdate = x));

        var updated = await pictureService.Update(dto, actualFile);

        Assert.That(updated, Is.Not.Null);

        pictureRepositoryMock.Received(1).Update(Arg.Is(picture), Arg.Any<Picture>());
        Assert.That(pictureToUpdate, Is.Not.Null);


        Assert.That(pictureToUpdate.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(pictureToUpdate.ContentType, Is.EqualTo("image/jpeg"));
        Assert.That(pictureToUpdate.FileExtension, Is.EqualTo(".jpg"));
        Assert.That(pictureToUpdate.EntityTag, Is.EqualTo(expectedETag));
        Assert.That(pictureToUpdate.File, Is.Not.Null);
        Assert.That(pictureToUpdate.File.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(pictureToUpdate.File.Value, Is.EqualTo(ms.ToArray()));

        var translations = pictureToUpdate.Translations.ToList();
        Assert.That(translations.Count, Is.EqualTo(3));

        Assert.That(translations[0].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));
        Assert.That(translations[0].Description, Is.EqualTo("DescriptionDe"));
        Assert.That(translations[0].Source, Is.EqualTo("SourceDe"));

        Assert.That(translations[1].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));
        Assert.That(translations[1].Description, Is.Null);
        Assert.That(translations[1].Source, Is.EqualTo("SourceEn"));

        Assert.That(translations[2].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[2].LanguageId, Is.EqualTo(4));
        Assert.That(translations[2].Description, Is.EqualTo("DescriptionIt"));
        Assert.That(translations[2].Source, Is.Null);
    }

    [Test]
    public void CanOnlyCreateWithPictureAndFile()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await pictureService.Create(null, null));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'dto')"));

        var exception2 = Assert.ThrowsAsync<ArgumentNullException>(async () => await pictureService.Create(dto, null));
        Assert.That(exception2, Is.Not.Null);
        Assert.That(exception2.Message, Is.EqualTo("Value cannot be null. (Parameter 'file')"));
    }

    [Test]
    public void CanOnlyUpdateWithProvidedPicture()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await pictureService.Update(null));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'dto')"));
    }

    [Test]
    public void CanOnlyUpdateWhenPictureAlreadyExists()
    {
        dto.EntryId = 2;
        var exception = Assert.ThrowsAsync<EntityNotFoundException>(async () => await pictureService.Update(dto));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Kein Eintrag mit der ID='2' vorhanden."));
    }

    [Test]
    public void ThrowsExceptionWhenImageHasUnsupportedType()
    {
        using var ms = new MemoryStream("TEST IMAGE"u8.ToArray());
        var brokenFile = new FormFile(ms, 0, ms.Length, "test-image", "test-image.jpg");

        var exception = Assert.ThrowsAsync<BusinessValidationException>(async () => await pictureService.Create(dto, brokenFile));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Das Dateiformat der Abbildung ist kein Bildformat oder wird nicht unterstützt."));
        Assert.That(exception.ModelKey, Is.EqualTo("file"));
    }

    [Test]
    public void ThrowsExceptionWhenImageHasWrongType()
    {
        using var ms = CreateOnePixelImage(SKEncodedImageFormat.Bmp);
        var bitmapFile = new FormFile(ms, 0, ms.Length, "test-image", "test-image.bmp");

        var exception = Assert.ThrowsAsync<BusinessValidationException>(async () => await pictureService.Create(dto, bitmapFile));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Das Dateiformat der Abbildung ist kein Bildformat oder wird nicht unterstützt."));
        Assert.That(exception.ModelKey, Is.EqualTo("file"));
    }

    [Test]
    public async Task UpdateCanHandleOnlyTextUpdates()
    {
        Picture pictureToUpdate = null;
        pictureRepositoryMock.Update(Arg.Any<Picture>(), Arg.Do<Picture>(x => pictureToUpdate = x));

        dto.SourceDe = null;
        var updated = await pictureService.Update(dto);

        Assert.That(updated, Is.Not.Null);

        pictureRepositoryMock.Received(1).Update(Arg.Is(picture), Arg.Any<Picture>());
        Assert.That(pictureToUpdate, Is.Not.Null);

        Assert.That(pictureToUpdate.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(pictureToUpdate.ContentType, Is.EqualTo(picture.ContentType));
        Assert.That(pictureToUpdate.FileExtension, Is.EqualTo(picture.FileExtension));
        Assert.That(pictureToUpdate.File, Is.Not.Null);
        Assert.That(pictureToUpdate.File.EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(pictureToUpdate.File.Value, Is.EqualTo(picture.File.Value));

        var translations = pictureToUpdate.Translations.ToList();
        Assert.That(translations.Count, Is.EqualTo(3));

        Assert.That(translations[0].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[0].LanguageId, Is.EqualTo(1));
        Assert.That(translations[0].Description, Is.EqualTo("DescriptionDe"));
        Assert.That(translations[0].Source, Is.EqualTo(null));

        Assert.That(translations[1].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[1].LanguageId, Is.EqualTo(2));
        Assert.That(translations[1].Description, Is.Null);
        Assert.That(translations[1].Source, Is.EqualTo("SourceEn"));

        Assert.That(translations[2].EntryId, Is.EqualTo(dto.EntryId));
        Assert.That(translations[2].LanguageId, Is.EqualTo(4));
        Assert.That(translations[2].Description, Is.EqualTo("DescriptionIt"));
        Assert.That(translations[2].Source, Is.Null);
    }

    [Test]
    public async Task MapPictureGetsCorrectFileExtensionAndContentType()
    {
        using var msPng = CreateOnePixelImage(SKEncodedImageFormat.Png);
        using var msGif = CreateOnePixelImage(SKEncodedImageFormat.Gif);
        using var msJpeg = CreateOnePixelImage(SKEncodedImageFormat.Jpeg);

        var bitmapFilePng = new FormFile(msPng, 0, msPng.Length, "test-image", "test-image.bmp");
        var bitmapFileGif = new FormFile(msGif, 0, msGif.Length, "test-image", "test-image.bmp");
        var bitmapFileJpeg = new FormFile(msJpeg, 0, msJpeg.Length, "test-image", "test-image.bmp");

        var pngPicture = await pictureService.Create(dto, bitmapFilePng);
        Assert.That(pngPicture.ContentType, Is.EqualTo("image/png"));
        Assert.That(pngPicture.FileExtension, Is.EqualTo(".png"));

        var gifPicture = await pictureService.Create(dto, bitmapFileGif);
        Assert.That(gifPicture.ContentType, Is.EqualTo("image/gif"));
        Assert.That(gifPicture.FileExtension, Is.EqualTo(".gif"));

        var jpegPicture = await pictureService.Create(dto, bitmapFileJpeg);
        Assert.That(jpegPicture.ContentType, Is.EqualTo("image/jpeg"));
        Assert.That(jpegPicture.FileExtension, Is.EqualTo(".jpg"));
    }

    private static FormFile GetFormFile()
    {
        var stream = File.OpenRead(Path.Join("Assets", "test-image.jpg"));

        return new FormFile(stream, 0, stream.Length, "test-image", "test-image.jpg");
    }

    private static MemoryStream CreateOnePixelImage(SKEncodedImageFormat format)
    {
        // SkiaSharp only encodes JPEG, PNG, and WebP; use hardcoded minimal bytes for other formats
        if (format == SKEncodedImageFormat.Bmp)
        {
            // Minimal 1x1 white BMP (24-bit)
            byte[] bmpBytes =
            [
                0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00
            ];
            return new MemoryStream(bmpBytes);
        }

        if (format == SKEncodedImageFormat.Gif)
        {
            // Minimal 1x1 white GIF89a
            byte[] gifBytes =
            [
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00,
                0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02,
                0x44, 0x01, 0x00, 0x3B
            ];
            return new MemoryStream(gifBytes);
        }

        using var bitmap = new SKBitmap(1, 1);
        bitmap.SetPixel(0, 0, SKColors.White);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        return new MemoryStream(data.ToArray());
    }
}
