using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IEntryValidationService
{
    void ValidateLanguageDetails(IEntryModification entryModification);
    Task<Collection> ValidateCollection(IEntryModification entryModification);
    void AuthorizeCollection(int collectionId, int? existingCollectionId = null);
    Task<Office> ValidateOffice(IEntryModification entryModification);
    Task<ConceptSystemPosition> ValidateConceptSystemPosition(IEntryModification entryModification);
    void ValidatePictureFile(PictureDetail picture, IFormFile pictureFile);
    Task<IList<LenochClassification>> ValidateLenochClassification(IEntryModification entryModification);
    Task<IList<Collection>> ValidateRelatedCollections(IEntryModification entryModification);
    Task<Picture> ValidatePicture(Picture clientPicture);
    Task<ICollection<Glossary>> ValidateGlossaries(IEntryModification entryModification);
    void AuthorizeGlossaries(IEnumerable<int> glossaryIds, IEnumerable<Glossary> existingGlossaries = null);
    Task<ICollection<EntryReference>> ValidateReferences(IEntryModification entryModification, Entry clientEntry);
    ICollection<EntryExternalLink> ValidateExternalLinks(IEntryModification entryModification, Entry clientEntry);
    Task<IList<Subject>> ValidateSubjects(IEntryModification entryModification);
    Task<IList<Domain.Models.Domain>> ValidateDomains(IEntryModification entryModification);
    Task<IList<AdminInfo>> ValidateAdminInfos(IEntryModification entryModification);
    void EscapeBracketsInHyperlinksInSources(EntryModificationLanguageDetail detail);
}
