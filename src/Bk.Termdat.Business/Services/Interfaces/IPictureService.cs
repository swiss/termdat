using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IPictureService
{
    Task<DomainModels.Picture> GetPicture(int entryId);
    Task<DomainModels.Picture> Create(ResourceModels.PictureDetail dto, IFormFile file);
    Task<DomainModels.Picture> Update(ResourceModels.PictureDetail dto, IFormFile file = null);
    Task Delete(int entryId);
}
