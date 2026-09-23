using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.Business.Public.Api.Resource.Models;

public class GetEntriesParameter
{
    /// <summary>
    /// Iso Code - Defines the language you want the metadata (collection, status, reliability, subject, office). Case insensitive, e.g. "DE" / "De" / "de" all yield the same result
    /// This will also include the entry details of this language (if available) in your result.
    /// Main languages: DE, FR, IT, EN, RM.
    /// All available languages:
    /// - DE = German
    /// - FR = French
    /// - IT = Italian
    /// - RM = Romansh
    /// - EN = English
    /// - LA = Latin
    /// - DA = Danish
    /// - FI = Finnish
    /// - NL = Dutch
    /// - PT = Portuguese
    /// - SV = Swedish
    /// - ES = Spanish
    /// - PL = Polish
    /// - CS = Czech
    /// - SK = Slovak
    /// - SL = Slovenian
    /// - HR = Croatian
    /// - HU = Hungarian
    /// - NO = Norwegian
    /// - IS = Icelandic
    /// - EL = Greek
    /// - TR = Turkish
    /// - KR = Korean
    /// - ZH = Chinese
    /// </summary>
    [Required]
    public string InLanguageCode { get; set; }

    /// <summary>
    /// Iso Code - Defines the language you will receive the entries language details (if available) in your results. Case insensitive, e.g. "DE" / "De" / "de" all yield the same result
    /// This parameter can be omitted. You will only receive details in <see cref="InLanguageCode"/>.
    /// Main languages: DE, FR, IT, EN, RM.
    /// All available languages:
    /// - DE = German
    /// - FR = French
    /// - IT = Italian
    /// - RM = Romansh
    /// - EN = English
    /// - LA = Latin
    /// - DA = Danish
    /// - FI = Finnish
    /// - NL = Dutch
    /// - PT = Portuguese
    /// - SV = Swedish
    /// - ES = Spanish
    /// - PL = Polish
    /// - CS = Czech
    /// - SK = Slovak
    /// - SL = Slovenian
    /// - HR = Croatian
    /// - HU = Hungarian
    /// - NO = Norwegian
    /// - IS = Icelandic
    /// - EL = Greek
    /// - TR = Turkish
    /// - KR = Korean
    /// - ZH = Chinese
    /// </summary>
    public string OutLanguageCode { get; set; }

    /// <summary>
    /// List of entry id's to get.
    /// </summary>
    [Required]
    public int[] EntryIds { get; set; }
}
