namespace Bk.Termdat.Business.Domain.Models;

public class NewsItemTranslation : Entity, IEntityTranslation
{
    public int NewsItemId { get; set; }
    public NewsItem NewsItem { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public required string Title { get; set; }
    public required string Text { get; set; }
}
