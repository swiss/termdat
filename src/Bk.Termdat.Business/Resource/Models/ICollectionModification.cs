namespace Bk.Termdat.Business.Resource.Models;

public interface ICollectionModification : ILanguageDescription
{
    string Code { get; set; }
    int? ClassificationId { get; set; }
    bool IsObsolete { get; set; }
    string? Description { get; set; }
    string? Documentation { get; set; }
    bool Active { get; set; }
    bool ActiveApi { get; set; }
    bool ExportLindas { get; set; }
}
