namespace Bk.Termdat.Business.Domain.Models;

public interface IEntityTranslation
{
    int LanguageId { get; set; }

    Language Language { get; set; }
}
