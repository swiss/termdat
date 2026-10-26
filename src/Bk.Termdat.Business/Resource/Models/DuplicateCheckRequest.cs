namespace Bk.Termdat.Business.Resource.Models;

public class DuplicateCheckRequest
{
    public required int LanguageId { get; set; }
    public int? CollectionId { get; set; }
    public int? GlossaryId { get; set; }
}
