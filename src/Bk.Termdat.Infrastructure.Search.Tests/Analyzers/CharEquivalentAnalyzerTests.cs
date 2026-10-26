using Bk.Termdat.Infrastructure.Search.Analyzers;
using Lucene.Net.Analysis;
using Lucene.Net.Util;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.Search.Tests.Analyzers;

[TestFixture]
internal class CharEquivalentAnalyzerTests : BaseTokenStreamTestCase
{
    private readonly CharEquivalentsAnalyzer analyzer = new(LuceneVersion.LUCENE_48);

    [TestCase("Confédération suisse", new[] { "confederation", "suisse" })]
    [TestCase("Hœ Hœ Hœ", new[] { "hoe", "hoe", "hoe" })]
    [TestCase("Hæ Hæ", new[] { "hae", "hae" })]
    [TestCase("Ÿ le fünnÿ", new[] { "y", "le", "funny" })]
    [TestCase("Ëllâ Âllë çÛgÔÇ", new[] { "ella", "alle", "cugoc" })]
    [TestCase("d’party d´party d'party di party", new[] { "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("d\"party d\'party d`party d´party dʹparty d‘party d’party d′party d″party d‴party d‵party d‶party d‷party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dʹparty dʺparty dʻparty dʼparty dʽparty dʾparty dʿparty dˊparty dˋparty dʿparty dˈparty d˝party dˮparty", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dꞌparty d«party d»party d‚party d‛party d“party d”party d„party d‟party d＇party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    public void TestCharEquivalentsFrench(string input, string[] expectedTokens)
    {
        input = CharEquivalentsAnalyzer.PreprocessEquivalentsText(input, 6);
        AssertAnalyzesTo(analyzer, input, expectedTokens);
    }

    [TestCase("codice di attivazione", new[] { "codice", "d", "attivazione" })]
    [TestCase("codice d'attivazione", new[] { "codice", "d", "attivazione" })]
    [TestCase("Confédération suisse", new[] { "confederation", "suisse" })]
    [TestCase("d’party d´party d'party di party diparty", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "diparty" })]
    [TestCase("d’party d´party d'party dparty", new[] { "d", "party", "d", "party", "d", "party", "dparty" })]
    [TestCase("un’party un´party un'party unparty", new[] { "un", "party", "un", "party", "un", "party", "unparty" })]
    [TestCase("d\"party d\'party d`party d´party dʹparty d‘party d’party d′party d″party d‴party d‵party d‶party d‷party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dʹparty dʺparty dʻparty dʼparty dʽparty dʾparty dʿparty dˊparty dˋparty dʿparty dˈparty d˝party dˮparty", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dꞌparty d«party d»party d‚party d‛party d“party d”party d„party d‟party d＇party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    public void TestCharEquivalentsItalian(string input, string[] expectedTokens)
    {
        input = CharEquivalentsAnalyzer.PreprocessEquivalentsText(input, 7);
        AssertAnalyzesTo(analyzer, input, expectedTokens);
    }

    [TestCase("d’party d´party d'party dparty daparty da partey", new[] { "d", "party", "d", "party", "d", "party", "dparty", "daparty", "d", "partey" })]
    [TestCase("in’party in´party in'party inparty inaparty ina partey", new[] { "in", "party", "in", "party", "in", "party", "inparty", "inaparty", "in", "partey" })]
    [TestCase("ch’party ch´party ch'party chparty cheparty che partey", new[] { "ch", "party", "ch", "party", "ch", "party", "chparty", "cheparty", "ch", "partey" })]
    [TestCase("d\"party d\'party d`party d´party dʹparty d‘party d’party d′party d″party d‴party d‵party d‶party d‷party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dʹparty dʺparty dʻparty dʼparty dʽparty dʾparty dʿparty dˊparty dˋparty dʿparty dˈparty d˝party dˮparty", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dꞌparty d«party d»party d‚party d‛party d“party d”party d„party d‟party d＇party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    public void TestCharEquivalentsRumantsch(string input, string[] expectedTokens)
    {
        input = CharEquivalentsAnalyzer.PreprocessEquivalentsText(input, 8);
        AssertAnalyzesTo(analyzer, input, expectedTokens);
    }

    [TestCase("Europäische Union", new[] { "europaische", "union" })]
    [TestCase("Wörter in Büchern", new[] { "worter", "in", "buchern" })]
    [TestCase("Örtliche Fleißigkeit", new[] { "ortliche", "fleissigkeit" })]
    [TestCase("ÄnDeruNgSwÜnScHe", new[] { "anderungswunsche" })]
    [TestCase("AEnderungswUensche", new[] { "anderungswunsche" })]
    [TestCase("Aendern uEberlOeSungen", new[] { "andern", "uberlosungen" })]
    [TestCase("Äder Öder Üder Oettinger Uettinger Aettinger aEt OEDER", new[] { "ader", "oder", "uder", "ottinger", "uttinger", "attinger", "at", "oder" })]
    [TestCase("d\"party d\'party d`party d´party dʹparty d‘party d’party d′party d″party d‴party d‵party d‶party d‷party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dʹparty dʺparty dʻparty dʼparty dʽparty dʾparty dʿparty dˊparty dˋparty dʿparty dˈparty d˝party dˮparty", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    [TestCase("dꞌparty d«party d»party d‚party d‛party d“party d”party d„party d‟party d＇party", new[] { "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party", "d", "party" })]
    public void TestCharEquivalentsGerman(string input, string[] expectedTokens)
    {
        input = CharEquivalentsAnalyzer.PreprocessEquivalentsText(input, 2);
        AssertAnalyzesTo(analyzer, input, expectedTokens);
    }
}
