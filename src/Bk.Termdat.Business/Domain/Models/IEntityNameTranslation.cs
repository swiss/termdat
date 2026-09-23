namespace Bk.Termdat.Business.Domain.Models;

public interface IEntityNameTranslation : IEntityTranslation
{
    string Name { get; set; }
}
