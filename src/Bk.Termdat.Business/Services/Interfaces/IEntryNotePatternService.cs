namespace Bk.Termdat.Business.Services.Interfaces;

public interface IEntryNotePatternService
{
    bool IsObsolete(string text);
    bool IsRare(string text);
    bool ShouldAvoid(string text);
    (bool isObsolete, bool isRare, bool shouldAvoid) DetermineEntryNotePatterns(string note);
}
