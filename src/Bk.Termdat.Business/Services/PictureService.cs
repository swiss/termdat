using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services;

public class PictureService : IPictureService
{
    private readonly IPictureRepository pictureRepository;
    private readonly ILanguageRepository languageRepository;
    private readonly IPictureTranslationMapper pictureTranslationMapper;

    public PictureService(IPictureRepository pictureRepository, ILanguageRepository languageRepository, IPictureTranslationMapper pictureTranslationMapper)
    {
        this.pictureRepository = pictureRepository;
        this.languageRepository = languageRepository;
        this.pictureTranslationMapper = pictureTranslationMapper;
    }

    public async Task<DomainModels.Picture> GetPicture(int entryId)
    {
        var picture = await pictureRepository.GetById(entryId);

        if (picture?.File == null)
        {
            return null;
        }

        return picture;
    }

    public Task<DomainModels.Picture> Create(ResourceModels.PictureDetail dto, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(file);

        return CreateInternal(dto, file);
    }

    private async Task<DomainModels.Picture> CreateInternal(ResourceModels.PictureDetail dto, IFormFile file)
    {
        var picture = await MapPicture(dto, file);
        await pictureRepository.Create(picture);

        return picture;
    }

    public async Task<DomainModels.Picture> Update(ResourceModels.PictureDetail dto, IFormFile file = null)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var existingPicture = await pictureRepository.GetByIdForUpdate(dto.EntryId.Value);
        if (existingPicture == null)
        {
            throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, BackendTexts.EntryNotFound, dto.EntryId.Value));
        }

        var pictureUpdate = await MapPicture(dto, file);

        // When image was not changed, but any description or source, file will be null
        if (pictureUpdate.File == null)
        {
            pictureUpdate.File = existingPicture.File;
            pictureUpdate.ContentType = existingPicture.ContentType;
            pictureUpdate.EntityTag = existingPicture.EntityTag;
            pictureUpdate.FileExtension = existingPicture.FileExtension;
        }

        pictureRepository.Update(existingPicture, pictureUpdate);

        return pictureUpdate;
    }

    public Task Delete(int entryId)
    {
        return pictureRepository.Delete(entryId);
    }

    private async Task<DomainModels.Picture> MapPicture(ResourceModels.PictureDetail dto, IFormFile file)
    {
        var picture = new DomainModels.Picture
        {
            EntryId = dto.EntryId.Value,
            Translations = await MapPictureTranslations(dto)
        };

        if (file == null)
        {
            return picture;
        }

        try
        {
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var format = SKCodec.Create(ms);

#pragma warning disable CA1508
            if (format is null)
            {
                throw new BusinessValidationException(nameof(file), BackendTexts.PictureFileType);
            }
#pragma warning restore CA1508

            if (format.EncodedFormat is SKEncodedImageFormat.Jpeg)
            {
                picture.ContentType = MediaTypeNames.Image.Jpeg;
                picture.FileExtension = ".jpg";
            }
            else if (format.EncodedFormat is SKEncodedImageFormat.Png)
            {
                picture.ContentType = "image/png";
                picture.FileExtension = ".png";
            }
            else if (format.EncodedFormat is SKEncodedImageFormat.Gif)
            {
                picture.ContentType = MediaTypeNames.Image.Gif;
                picture.FileExtension = ".gif";
            }
            else
            {
                throw new BusinessValidationException(nameof(file), BackendTexts.PictureFileType);
            }

            picture.File = new DomainModels.PictureFile
            {
                Value = ms.ToArray(),
                EntryId = picture.EntryId
            };

            picture.EntityTag = PictureEntityTagService.ComputeEntityTag(picture.File.Value);
            return picture;
        }
        catch (Exception ex) when (ex is ArgumentNullException or NotSupportedException)
        {
            throw new BusinessValidationException(
                nameof(file),
                BackendTexts.PictureFileType);
        }
    }

    private async Task<ICollection<DomainModels.PictureTranslation>> MapPictureTranslations(ResourceModels.PictureDetail dto)
    {
        var languages = (await languageRepository.GetAll()).ToList();

        return pictureTranslationMapper.Map(dto, languages).ToList();
    }
}
