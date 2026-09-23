using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IClassificationService
{
    Task<IEnumerable<ClassificationSummary>> GetAll();
}
