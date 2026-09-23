using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ILindasMapper
{
    Collection Map(Business.Domain.Models.Collection collection);
}
