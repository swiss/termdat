using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
namespace Bk.Termdat.Business.Services.Interfaces;

public interface ILenochClassificationService
{
    Task<IEnumerable<LenochClassificationSummary>> GetAll();
    Task<IEnumerable<LenochClassificationDetail>> GetAllDetails();
}
