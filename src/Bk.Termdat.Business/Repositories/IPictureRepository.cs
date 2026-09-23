using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IPictureRepository
{
    Task<Picture> GetById(int entryId);
    Task<Picture> GetByIdForUpdate(int entryId);
    Task<Picture> Create(Picture picture);
    Task Delete(int entryId);
    void Update(Picture existing, Picture update);
}
