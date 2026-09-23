using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Bk.Termdat.Infrastructure.Search.Analyzers;

public sealed class WordReplacementFilter : TokenFilter
{
    private readonly ICharTermAttribute termAttribute;

    private readonly Dictionary<char[], char[]> equivalents = new()
    {
        { new[] { 'd', 'i' }, new[] { 'd' } }, // IT: di d’d´ d'
        { new[] { 'd', 'a' }, new[] { 'd' } }, // RM: da d’ d´ d'
        { new[] { 'l', 'a' }, new[] { 'l' } }, // RM: la l’ l´ l'
        { new[] { 'i', 'n', 'a' }, new[] { 'i', 'n' } }, // RM: ina in’ in´ in'
        { new[] { 'c', 'h', 'e' }, new[] { 'c', 'h' } }, // RM: che ch’ ch´ ch'
    };

    public WordReplacementFilter(TokenStream input) : base(input)
    {
        termAttribute = AddAttribute<ICharTermAttribute>();
    }

    public override bool IncrementToken()
    {
        if (!m_input.IncrementToken())
        {
            return false;
        }

        int i;
        foreach (var (search, replace) in equivalents)
        {
            i = 0;
            while (i < search.Length && i < termAttribute.Length && search[i] == termAttribute[i])
            {
                i++;
            }

            if (i == search.Length && termAttribute.ToString() == new string(search))
            {
                termAttribute.SetEmpty();
                termAttribute.Append(new string(replace));
                break; // Match found, so we won't need to check the remaining equivalents
            }
        }

        return true;
    }
}
