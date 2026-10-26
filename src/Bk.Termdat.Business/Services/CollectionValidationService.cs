using System;
using System.Globalization;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Services;

public class CollectionValidationService : ICollectionValidationService
{
    private readonly IClassificationRepository classificationRepository;

    public CollectionValidationService(IClassificationRepository classificationRepository)
    {
        this.classificationRepository = classificationRepository;
    }

    public async Task<Classification> ValidateClassification(ICollectionModification collectionModification)
    {
        ArgumentNullException.ThrowIfNull(collectionModification);

        if (collectionModification.ActiveApi && collectionModification.ClassificationId == null)
        {
            throw new BusinessValidationException(
                nameof(CollectionCreate.ActiveApi), BackendTexts.ActiveApiSetWithoutId);
        }

        if (collectionModification.ClassificationId != null)
        {
            var requestedClassification = await classificationRepository.GetById(collectionModification.ClassificationId.Value);
            if (requestedClassification == null)
            {
                throw new BusinessValidationException(
                    nameof(CollectionCreate.ClassificationId),
                    string.Format(CultureInfo.InvariantCulture, BackendTexts.ClassificationNotFound, collectionModification.ClassificationId));
            }

            return requestedClassification;
        }
        return null;
    }
}
