using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ICollectionValidationService
{
    Task<Domain.Models.Classification> ValidateClassification(ICollectionModification collectionModification);
}
