using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IUserAccountValidationService
{
    Task<ICollection<Glossary>> ValidateGlossaries(IUserAccountModification userAccountModification);
    Task<ICollection<Collection>> ValidateCollections(IUserAccountModification userAccountModification);
}
