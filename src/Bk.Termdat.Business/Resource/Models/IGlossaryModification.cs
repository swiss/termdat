namespace Bk.Termdat.Business.Resource.Models;

public interface IGlossaryModification : ILanguageDescription
{
    string Code { get; set; }
    string? Description { get; set; }
}
