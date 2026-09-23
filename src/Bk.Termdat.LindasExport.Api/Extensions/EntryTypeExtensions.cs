using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Extensions;

public static class EntryTypeExtensions
{
    public static string GetUri(this EntryType entryType)
    {
        return entryType switch
        {
            EntryType.Name => Uris.NameType,
            EntryType.Abbreviation => Uris.AbbreviationType,
            EntryType.Term => Uris.TermType,
            EntryType.Phraseology => Uris.PhraseologyType,
            EntryType.None or _ => throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null)
        };
    }

    public static string GetUriAddition(this EntryType entryType)
    {
        return entryType switch
        {
            EntryType.Abbreviation => Uris.TermdatAbbreviationUriAddition,
            EntryType.Term => Uris.TermdatTermUriAddition,
            EntryType.Phraseology => Uris.TermdatPhraseologyUriAddition,
            EntryType.Name or EntryType.None or _ => throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null)
        };
    }
}
