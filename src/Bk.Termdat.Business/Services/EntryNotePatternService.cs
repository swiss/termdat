using System.Text;
using System.Text.RegularExpressions;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

// .Contains() checks are done to avoid regex matching for most texts since they are very expensive and relatively slow
public class EntryNotePatternService : IEntryNotePatternService
{
    private readonly EntryNotePatternOptions entryNotePatternOptions;
    private readonly Regex isObsoleteRegex;
    private readonly Regex isRareRegex;
    private readonly Regex shouldAvoidRegex;

    public EntryNotePatternService(IOptions<EntryNotePatternOptions> entryNotePatternOptions)
    {
        this.entryNotePatternOptions = entryNotePatternOptions.Value;

        var obsoleteRegexString = GetRegexString(this.entryNotePatternOptions.ObsoletePattern, this.entryNotePatternOptions.RarePattern, this.entryNotePatternOptions.ShouldAvoidPattern);
        isObsoleteRegex = new Regex(obsoleteRegexString,
        RegexOptions.Compiled | RegexOptions.Singleline);

        var rareRegexString = GetRegexString(this.entryNotePatternOptions.RarePattern, this.entryNotePatternOptions.ObsoletePattern, this.entryNotePatternOptions.ShouldAvoidPattern);
        isRareRegex = new Regex(rareRegexString,
        RegexOptions.Compiled | RegexOptions.Singleline);

        var shouldAvoidRegexString = GetRegexString(this.entryNotePatternOptions.ShouldAvoidPattern, this.entryNotePatternOptions.RarePattern, this.entryNotePatternOptions.ObsoletePattern);
        shouldAvoidRegex = new Regex(shouldAvoidRegexString,
        RegexOptions.Compiled | RegexOptions.Singleline);
    }

    private string GetRegexString(string pattern, string otherPattern1, string otherPattern2)
    {
        var regexBuilder = new StringBuilder();
        regexBuilder.Append(entryNotePatternOptions.StartingPattern);
        regexBuilder.Append(@"[\s]?");
        regexBuilder.Append($"({otherPattern1}|{otherPattern2})?");
        regexBuilder.Append(@"[,\s]*[,'""?!:\.\/]?(");
        regexBuilder.Append(pattern);
        regexBuilder.Append(@")[\W]?");

        return regexBuilder.ToString();
    }

    public bool IsObsolete(string text)
    {
        return text != null && text.Contains(entryNotePatternOptions.StartingPattern) && isObsoleteRegex.IsMatch(text);
    }

    public bool IsRare(string text)
    {
        return text != null && text.Contains(entryNotePatternOptions.StartingPattern) && isRareRegex.IsMatch(text);
    }

    public bool ShouldAvoid(string text)
    {
        return text != null && text.Contains(entryNotePatternOptions.StartingPattern) && shouldAvoidRegex.IsMatch(text);
    }

    public (bool isObsolete, bool isRare, bool shouldAvoid) DetermineEntryNotePatterns(string note)
    {
        return (IsObsolete(note), IsRare(note), ShouldAvoid(note));
    }
}
