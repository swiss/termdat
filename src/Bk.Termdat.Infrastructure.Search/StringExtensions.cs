using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.QueryParsers.Classic;

namespace Bk.Termdat.Infrastructure.Search;

public static class StringExtensions
{
    // https://lucenenet.apache.org/docs/4.8.0-beta00015/api/queryparser/Lucene.Net.QueryParsers.Classic.html#escaping-special-characters
    private static readonly HashSet<char> SpecialChars = new()
    {
        '+', '\\', '-', '(', ')', '[', ']', '{', '}', '&', '|', '!', '?', '*', '^', '~', ':', '‐', '/', '"', '.'
    };

    private static readonly HashSet<string> SpecialWords = new()
    {
        "AND", "OR", "NOT"
    };

    public static IEnumerable<string> ToSanitizedLucenePhrases(this string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return Array.Empty<string>();
        }

        foreach (var character in SpecialChars)
        {
            phrase = phrase.Replace(character, ' ');
        }

        var escapedPhrase = phrase.RemoveRecurringSpaces().Trim();

        var phrases = escapedPhrase.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        return phrases
            .Where(s => !SpecialWords.Contains(s.ToUpperInvariant()))
            .Select(QueryParserBase.Escape);
    }

    public static string ToSanitizedLucenePhrase(this string phrase)
    {
        return string.Join(" ", ToSanitizedLucenePhrases(phrase));
    }

    public static string RemoveRecurringSpaces(this string str)
    {
        str ??= string.Empty;

        int previousLength;
        int length;

        do
        {
            previousLength = str.Length;
            str = str.Replace("  ", " ", StringComparison.OrdinalIgnoreCase);
            length = str.Length;
        } while (length < previousLength);

        return str;
    }
}
