using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.Business.Public.Api.Resource.Models;

public class SearchParameter
{
    /// <summary>
    /// IDs of the collections to search in.
    /// If no ID is given, a default set of collections will be included (=VARIA).
    /// </summary>
    public int[] CollectionIds { get; set; }

    /// <summary>
    /// IDs of the classifications to search in.
    /// If no ID is given, a default set of classifications will be included (=VARIA).
    /// </summary>
    public int[] ClassificationIds { get; set; }

    /// <summary>
    /// Define the field in which the API is searching for the given search term. If not set, defaults to Field.Terminus = true, Field.Name = true, Field.Abbreviation = true, Field.Phraseology = true
    /// </summary>
    public FieldSelection Field { get; set; } = new();

    /// <summary>
    /// Iso Code - Defines the language you want to search in. Case insensitive, e.g. "DE" / "De" / "de" all yield the same result
    /// All the metadata (e.g. office, collection, classification, etc.) will be provided in this language.
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
    /// Maximum number of entries returned by the search.
    /// Default: 25
    /// Maximum: 1000
    /// </summary>
    public int MaxEntryCount { get; set; } = 25;

    /// <summary>
    /// Iso Code - Defines the language you want additional language entry results for your matched result. Case insensitive, e.g. "DE" / "De" / "de" all yield the same result
    /// This parameter can be omitted, this will mimic the V1 API ReturnType=Summary. You will only receive details in <see cref="InLanguageCode"/>.
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
    /// Search term in Lucene Query Syntax: use the *, ? wildcards or the fuzzy operator ~
    /// See: https://lucenenet.apache.org/docs/4.8.0-beta00015/api/queryparser/Lucene.Net.QueryParsers.Classic.html
    /// Examples:
    /// - Exact:       Bund
    /// - Begins with: Schweiz*
    /// - Ends with:   *amt
    /// - Contains:    *term*
    /// - Fuzzy:       term~
    /// - Phrase:      "term term"
    /// </summary>
    [Required]
    public string SearchTerm { get; set; }

    /// <summary>
    /// (Default) Detail: complete entries with synonyms in <see cref="InLanguageCode"/> and <see cref="OutLanguageCode"/>.
    /// Summary: entries with only metadata, without synonyms. Includes "hits" that tell you what matched within the synonyms.
    /// </summary>
    [Required]
    public ReturnType ReturnType { get; set; }
}
