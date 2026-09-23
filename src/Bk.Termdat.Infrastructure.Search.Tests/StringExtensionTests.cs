using System.Collections.Generic;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.Search.Tests;

[TestFixture]
public class StringExtensionTests
{
    [TestCase("test AND phrase OR NOT abc", new[] { "test", "phrase", "abc" })]
    [TestCase("      phrase         with    spaces       ", new[] { "phrase", "with", "spaces" })]
    [TestCase("(phrase:&) -test@+ \\[*abc^1]!{0}? new~", new[] { "phrase", "test@", "abc", "1", "0", "new" })]
    [TestCase("phrase", new[] { "phrase" })]
    [TestCase("", new string[0])]
    [TestCase("EntryId:[1 TO ]", new[] { "EntryId", "1", "TO" })]
    [TestCase("bund^", new[] { "bund" })]
    [TestCase("bund AND OR bund", new[] { "bund", "bund" })]
    [TestCase("bund\\", new[] { "bund" })]
    public void ToSanitizedLucenePhrases_ShouldSplitAndSanitizePhrase(string phrase, IEnumerable<string> expected)
    {
        var phrases = phrase.ToSanitizedLucenePhrases();
        Assert.That(phrases, Is.EqualTo(expected));
    }

    [TestCase("test AND phrase OR NOT abc", "test phrase abc")]
    [TestCase("      phrase         with    spaces       ", "phrase with spaces")]
    [TestCase("(phrase:&) -test@+ \\[*abc^1]!{0}? new~", "phrase test@ abc 1 0 new")]
    [TestCase("phrase", "phrase")]
    [TestCase("", "")]
    [TestCase("EntryId:[1 TO ]", "EntryId 1 TO")]
    [TestCase("bund^", "bund")]
    [TestCase("bund AND OR bund", "bund bund")]
    [TestCase("bund\\", "bund")]
    public void ToSanitizedLucenePhrase_ShouldSanitizePhrase(string phrase, string expected)
    {
        var phrases = phrase.ToSanitizedLucenePhrase();
        Assert.That(phrases, Is.EqualTo(expected));
    }
}
