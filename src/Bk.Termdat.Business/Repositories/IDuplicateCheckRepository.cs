using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IDuplicateCheckRepository
{
    Task<IReadOnlyList<DuplicateCheckMatch>> GetByCollectionIdAndLanguageId(int collectionId, int languageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DuplicateCheckMatch>> GetByGlossaryIdAndLanguageId(int glossaryId, int languageId, CancellationToken cancellationToken = default);
}
