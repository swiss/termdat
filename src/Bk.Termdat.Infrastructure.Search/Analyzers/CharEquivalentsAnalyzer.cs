using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace Bk.Termdat.Infrastructure.Search.Analyzers;

public class CharEquivalentsAnalyzer : StopwordAnalyzerBase
{
    private const int DefaultMaxTokenLength = 255;
    private const int GermanLanguageId = 2;

    private static readonly char[] QuotationMarkUnicodeCharacters = {
        '"', // U+0022
        '\'', // U+0027
        '`', // U+0060
        '´', // U+00B4
        'ʹ', // U+0374
        '‘', // U+2018
        '’', // U+2019
        '′', // U+2032
        '″', // U+2033
        '‴', // U+2034
        '‵', // U+2035
        '‶', // U+2036
        '‷', // U+2037
        'ʹ', // U+02B9
        'ʺ', // U+02BA
        'ʻ', // U+02BB
        'ʼ', // U+02BC
        'ʽ', // U+02BD
        'ʾ', // U+02BE
        'ʿ', // U+02BF
        'ˊ', // U+02CA
        'ˋ', // U+02CB
        'ʿ', // U+02BF
        'ˈ', // U+02C8
        '˝', // U+02DD
        'ˮ', // U+02EE
        'ꞌ', // U+A78C
        '«', // U+00AB
        '»', // U+00BB
        '‚', // U+201A
        '‛', // U+201B
        '“', // U+201C
        '”', // U+201D
        '„', // U+201E
        '‟', // U+201F
        '＇', // U+FF07
    };

    private static readonly IDictionary<string, string> GermanEquivalents = new Dictionary<string, string>()
    {
        {"ae", "ä"},
        {"oe", "ö"},
        {"ue", "ü"},
    };

    public CharEquivalentsAnalyzer(LuceneVersion version) : base(version)
    {
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var tokenizer = new StandardTokenizer(m_matchVersion, reader)
        {
            MaxTokenLength = DefaultMaxTokenLength
        };

        TokenStream tokenStream = new StandardFilter(m_matchVersion, tokenizer);
        tokenStream = new ICUFoldingFilter(tokenStream); // Replaces accents and special characters with their basic ascii equivalent, e.g. é -> e, ä -> a, ß => ss etc
        tokenStream = new LowerCaseFilter(m_matchVersion, tokenStream);
        tokenStream = new WordReplacementFilter(tokenStream); // Replaces multiple char sequences with specific equivalent, e.g. ina -> in, di -> d
        tokenStream = new StopFilter(m_matchVersion, tokenStream, m_stopwords);
        return new TokenStreamComponents(tokenizer, tokenStream);
    }

    public static string PreprocessEquivalentsText(string text, int languageId = 0)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        text = ReplaceQuotationMarks(text);

        return languageId switch
        {
            GermanLanguageId => ReplaceEquivalents(GermanEquivalents, text),
            _ => text
        };
    }

    private static string ReplaceQuotationMarks(string text)
    {
        foreach (var mark in QuotationMarkUnicodeCharacters)
        {
            text = text.Replace(mark, ' ');
        }

        return text;
    }

    private static string ReplaceEquivalents(IDictionary<string, string> dictionary, string text)
    {
        foreach (var equivalent in dictionary)
        {
            text = text.Replace(equivalent.Key, equivalent.Value, StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }
}
