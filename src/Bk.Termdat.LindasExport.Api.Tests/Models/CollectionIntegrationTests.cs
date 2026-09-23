using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Models;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Collection = Bk.Termdat.LindasExport.Api.Models.Collection;
using Domain = Bk.Termdat.LindasExport.Api.Models.Domain;
using Entry = Bk.Termdat.LindasExport.Api.Models.Entry;
using Glossary = Bk.Termdat.LindasExport.Api.Models.Glossary;
using Language = Bk.Termdat.LindasExport.Api.Models.Language;
using Office = Bk.Termdat.LindasExport.Api.Models.Office;
using ReferenceType = Bk.Termdat.LindasExport.Api.Models.ReferenceType;
using Reliability = Bk.Termdat.LindasExport.Api.Models.Reliability;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace Bk.Termdat.LindasExport.Api.Tests.Models;

[TestFixture]
internal class CollectionIntegrationTests
{
    private static readonly Entry _entry109123 = new()
    {
        Id = 109123,
        Modified = new DateTime(2024, 05, 13, 08, 30, 59),
        AdminInfoCodeItems = new List<string> { "AdminInfoCodeA" },
        Status = Status.Validated,
        Office =
            new Office
            {
                Code = "A-Office",
                Names =
                    new List<Text>
                    {
                        new() { Value = "A-DE", Language = Language.De },
                        new() { Value = "A-FR", Language = Language.Fr },
                        new() { Value = "A-IT", Language = Language.It },
                        new() { Value = "A-EN", Language = Language.En }
                    }
            },
        Glossaries =
            new List<Glossary>
            {
                new()
                {
                    Code = "B-Glossary",
                    Names =
                        new List<Text>
                        {
                            new() { Value = "B-Glossary-DE", Language = Language.De },
                            new() { Value = "B-Glossary-FR", Language = Language.Fr },
                            new() { Value = "B-Glossary-IT", Language = Language.It },
                            new() { Value = "B-Glossary-EN", Language = Language.En }
                        }
                },
                new()
                {
                    Code = "C-Glossary",
                    Names =
                        new List<Text> { new() { Value = "C-Glossary-EN", Language = Language.En } }
                }
            },
        Reliability =
            new Reliability
            {
                Code = ((int)Business.Domain.Models.Reliability.Translated).ToString(),
                Names = Reliability.GetNamesByReliability(Business.Domain.Models.Reliability.Translated)
            },
        References = new List<Reference> { new() { Id = 109122, Type = ReferenceType.General } },
        Parts = new List<EntryPart>
        {
            new()
            {
                Sequence = 1,
                Language = Language.De,
                Term = "Motion",
                TermCitation = "Parlamentsgesetz, Tit. vor Art. 120 (SR 171.10, Stand 2016-03)",
                Abbreviation = "Mo.",
                AbbreviationCitation =
                    "BR, Ber. in Erfüllung der Mo. Gutzwiller (11.3562), Konzipierung und Umsetzung von Fördermassnahmen für die Nutzung der tiefen Geothermie in der Schweiz, 2017-03-03, S. 21, Ziff. 2.3 (Internet, 2017-05-02)",
                Definition =
                    "Parlamentarischer Vorstoss, der den Bundesrat beauftragt, der Bundesversammlung einen Entwurf zu einem Erlass vorzulegen oder eine entsprechende Massnahme zu treffen."
            },
            new()
            {
                Sequence = 1,
                Language = Language.Fr,
                Term = "motion",
                TermCitation = "LF Parlement, tit. avant art. 120 (RS 171.10, état 2016-03)",
                Abbreviation = "mo.",
                AbbreviationCitation = "Ass. féd., Interventions parlementaires (Internet, 2021-05-20)",
                Definition =
                    "Intervention parlementaire par laquelle le Conseil fédéral est chargé de proposer à l'Assemblée fédérale un projet d'acte ou de prendre une mesure correspondante."
            },
            new()
            {
                Sequence = 1,
                Language = Language.It,
                Term = "mozione",
                TermCitation = "LF Parlamento, tit. prec. art. 120 (RS 171.10, stato 2016-03)",
                Abbreviation = "Mo.",
                AbbreviationCitation =
                    "CaF, Istr. Redazione dei testi ufficiali in italiano, 2003-09-16, p. 18, n. 5.2.2 (Internet, 2017-05-02)",
                Definition =
                    "Intervento parlamentare che incarica il Consiglio federale di presentare all'Assemblea federale un disegno d'atto legislativo o di prendere i relativi provvedimenti."
            },
            new()
            {
                Sequence = 1,
                Language = Language.En,
                Term = "motion",
                TermCitation = "Parliament Act, tit. before art. 120 (SR 171.10, transl., status 2016-03)",
                Abbreviation = "Mo.",
                AbbreviationCitation = "FCh, Terminology Section, 2016",
                Definition =
                    "Parliamentary procedural request instructing the Federal Council to submit a specific draft bill to the Federal Assembly or to take appropriate measures."
            }
        }
    };

    private static readonly Entry _entry109117 = new()
    {
        Id = 109117,
        Modified = new DateTime(2023, 12, 31, 23, 59, 59),
        Status = Status.InProgress,
        Office =
            new Office
            {
                Code = "B-Office",
                Names =
                    new List<Text>
                    {
                        new() { Value = "B-DE", Language = Language.De },
                        new() { Value = "B-FR", Language = Language.Fr }
                    }
            },
        Reliability = new Reliability
        {
            Code = ((int)Business.Domain.Models.Reliability.FirstDraft).ToString(),
            Names = Reliability.GetNamesByReliability(Business.Domain.Models.Reliability.FirstDraft)
        },
        Domains = new List<Domain>
        {
            new()
            {
                Id = 42,
                Names =
                    new List<Text>
                    {
                        new() { Value = "42-DE", Language = Language.De },
                        new() { Value = "42-FR", Language = Language.Fr }
                    },
                ChildDomains = new List<Domain>
                {
                    new()
                    {
                        Id = 11,
                        Names = new List<Text>
                        {
                            new() { Value = "11-DE", Language = Language.De },
                            new() { Value = "11-FR", Language = Language.Fr }
                        }
                    },
                    new() { Id = 22, Names = new List<Text> { new() { Value = "22-EN", Language = Language.En } } }
                }
            },
            new()
            {
                Id = 666,
                Names =
                    new List<Text>
                    {
                        new() { Value = "666-DE", Language = Language.De },
                        new() { Value = "666-FR", Language = Language.Fr }
                    }
            }
        },
        References =
            new List<Reference>
            {
                new() { Id = 109116, Type = ReferenceType.None },
                new() { Id = 109119, Type = ReferenceType.Antonym }
            },
        Parts = new List<EntryPart>
        {
            new()
            {
                Sequence = 1,
                Language = Language.De,
                Term = "unbedingte Immunität",
                TermCitation = "Die Bundesversammlung, 1984, S. 30",
                Definition =
                    "Privileg der eidgenössischen Parlamentarier, nach welchem sie für die in der Bundesversammlung und in den Kommissionen abgegebenen Voten nicht verantwortlich gemacht werden können.",
                DefinitionCitation = "Privileg der eidgenössischen Parlamentarier, test-citation-1-de",
                Deprecated = true,
                Note = "test-note-1-de",
                NoteCitation = "test-note-citation-1-de",
                Context = "test-context-1-de",
                Country = "test-country-1-de"
            },
            new()
            {
                Sequence = 1,
                Language = Language.Fr,
                Term = "irresponsabilité absolue",
                TermCitation = "L'Assemblée fédérale, 1984, p. 28",
                Definition =
                    "Privilège parlementaire selon lequel un député fédéral ne peut être tenu responsable pour les opinions qu'il émet au sein de l'Assemblée fédérale ou de ses commissions.",
                Deprecated = true
            },
            new()
            {
                Sequence = 1,
                Language = Language.It,
                Term = "irresponsabilità assoluta",
                TermCitation = "L'Assemblea federale, 1984, p. 28",
                Definition =
                    "Privilegio parlamentare secondo il quale i deputati federali non possono essere chiamati a rispondere dei pareri da loro espressi in seno all'Assemblea federale o alle sue commissioni.",
                Deprecated = true
            },
            new()
            {
                Sequence = 1,
                Language = Language.En,
                Term = "absolute immunity",
                TermCitation = "The Swiss Federal Assembly, 1984, p. 30",
                Definition =
                    "Parliamentary privilege which says that no representative of the Swiss Federal Assembly is liable to criminal or civil sanctions for any opinion he expresses during council sessions.",
                Deprecated = true
            }
        }
    };

    private static readonly Entry _entry388009 = new()
    {
        Id = 388009,
        Modified = new DateTime(2000, 01, 01, 00, 01, 01),
        Office =
            new Office
            {
                Code = "C-Office",
                Names = new List<Text> { new() { Value = "C-EN", Language = Language.En } }
            },
        Reliability = new Reliability
        {
            Code = ((int)Business.Domain.Models.Reliability.Verified).ToString(),
            Names = Reliability.GetNamesByReliability(Business.Domain.Models.Reliability.Verified)
        },
        References =
            new List<Reference>
            {
                new() { Id = 388010, Type = ReferenceType.Specific },
                new() { Id = 388012, Type = ReferenceType.General }
            },
        Parts = new List<EntryPart>
        {
            new()
            {
                Sequence = 1,
                Language = Language.De,
                Term = "Einheit der elektrischen Stromstärke",
                TermCitation =
                    "PTB-Mitteilungen, Das Internationale Einheitensystem (SI), Heft 2, 2007, S. 156 Abs. 2.1.1.4",
                Name = "Ampere",
                NameCitation = "Einheitenverordnung, Art. 6 (SR 941.202, Stand 2013-01)",
                Abbreviation = "A",
                AbbreviationCitation = "Einheitenverordnung, Art. 6 (SR 941.202, Stand 2013-01)",
                Definition =
                    "Stärke eines zeitlich unveränderlichen elektrischen Stromes, der, durch zwei im Vakuum parallel im Abstand 1 Meter voneinander angeordnete, geradlinige, unendlich lange Leiter von vernachlässigbar kleinem, kreisförmigem Querschnitt fliessend, zwischen diesen Leitern je 1 Meter Leiterlänge die Kraft 2 · 10 -7 Newton hervorrufen würde."
            },
            new()
            {
                Sequence = 1,
                Language = Language.Fr,
                Term = "unité d'intensité du courant électrique",
                TermCitation =
                    "Techniques de l'ingénieur, Organisation et vocabulaire de la métrologie, Étalons métrologiques fondamentaux - Étalons électriques, al. 5, 2015 (Internet, 2016-01-25)",
                Name = "ampère",
                NameCitation = "O Unités, art. 6 (RS 941.202, état 2013-01)",
                Abbreviation = "A",
                AbbreviationCitation = "O Unités, art. 6 (RS 941.202, état 2013-01)",
                Definition =
                    "Intensité d'un courant constant qui, maintenu dans deux conducteurs parallèles, rectilignes, de longueur infinie, de section circulaire négligeable et placés à une distance de 1 mètre l'un de l'autre dans le vide, produirait entre ces conducteurs une force égale à 2 · 10 -7 newton par mètre de longueur."
            },
            new()
            {
                Sequence = 1,
                Language = Language.It,
                Term = "unità d'intensità di corrente elettrica",
                TermCitation = "Treccani, Sistemi di Unità, 2016, (Internet, 2016-01-25/))",
                Name = "ampère (a); ampere (b)",
                NameCitation =
                    "(a) O Unità, art. 6 (RS 941.202, stato 2013-01); (b) Treccani, Sistemi di Unità, 2016, (Internet, 2016-01-25/))",
                Abbreviation = "A",
                AbbreviationCitation = "O Unità, art. 6 (RS 941.202, stato 2013-01)",
                Definition =
                    "Intensità di una corrente elettrica costante che, percorrendo due conduttori paralleli rettilinei, di lunghezza infinita, di sezione circolare trascurabile, posti alla distanza di un metro l'uno dall'altro nel vuoto, produrrebbe fra questi conduttori una forza uguale a 2 · 10 -7 newton su ogni metro di lunghezza."
            },
            new()
            {
                Sequence = 1,
                Language = Language.En,
                Term = "unit of electric current",
                TermCitation = "Wikipedia, Ampere, 2016-02-23 (Internet, 2016-02-29)",
                Name = "ampere",
                NameCitation = "Wikipedia, Ampere, 2016-02-23 (Internet, 2016-02-29)",
                Abbreviation = "A",
                AbbreviationCitation = "Wikipedia, Ampere, 2016-02-23 (Internet, 2016-02-29)",
                Definition =
                    "Parliamentary procedural request instructing the Federal Council to submit a specific draft bill to the Federal Assembly or to take appropriate measures.",
                Note = "ampere-note-1-en",
                Context = "ampere-context-1-en",
                Country = "ampere-country-1-en"
            },
            new()
            {
                Sequence = 2,
                Language = Language.En,
                Term = "unity of electric current",
                TermCitation =
                    "NIST, The NIST Reference on Constants, Units, and Uncertainty, International System of Units (SI), Historical context, Unit of electric current, 2016 (Internet, 2016-01-25)",
                Note = "ampere-note-2-en",
                Context = "ampere-context-2-en",
                Country = "ampere-country-2-en"
            }
        }
    };

    private static readonly Collection _collectionMET16 = new()
    {
        Code = "MET16",
        Names = new List<Text>
        {
            new() { Value = "Terminologie der Metrologie", Language = Language.De },
            new() { Value = "Terminology of Metrology", Language = Language.En },
            new() { Value = "Terminologie de la métrologie", Language = Language.Fr },
            new() { Value = "Terminologia della metrologia", Language = Language.It }
        }
    };

    private static readonly Collection _collectionPFS17 = new()
    {
        Code = "PFS17",
        Names = new List<Text>
        {
            new() { Value = "Schweizerische parlamentarische Terminologie", Language = Language.De },
            new() { Value = "Swiss parliamentary terminology", Language = Language.En },
            new() { Value = "Terminologie parlementaire suisse", Language = Language.Fr },
            new() { Value = "Terminologia parlamentare svizzera", Language = Language.It }
        }
    };

    [Test]
    public void GetTriples_WithInconsistentDataOnMainParts_ShouldAddAvailableDataAnyway()
    {
        Collection collection = new()
        {
            Code = "Collection-Code",
            Entries = new List<Entry>
            {
                new()
                {
                    Id = 666,
                    Office = new Office { Code = "Office-Code" },
                    Reliability = new Reliability { Code = "Reliability-Code" },
                    Parts = new List<EntryPart>
                    {
                        new()
                        {
                            Sequence = 1,
                            Language = Language.De,
                            Abbreviation = "S1 Abbreviation only DE",
                            AbbreviationCitation = "S1 Abbreviation only citation DE",
                            Definition = "S1 definition DE"
                        },
                        new()
                        {
                            Sequence = 1,
                            Language = Language.Fr,
                            Term = "S1 Term FR",
                            TermCitation = "S1 Term citation FR",
                            Abbreviation = "S1 Abbreviation FR",
                            AbbreviationCitation = "S1 Abbreviation citation FR",
                            Definition = "S1 definition FR"
                        }
                    }
                }
            }
        };
        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = collection.GetTriples(graph, false);
        graph.Assert(triples);

        var writer = new NTriplesWriter();
        var nTriplesData = StringWriter.Write(graph, writer);
        var expectedData = File.ReadAllText("Models/TestData/BestEffortOutput");

        Assert.That(nTriplesData, Is.EqualTo(expectedData));
    }

    [Test]
    public void TurtleFileToTripleHelperTest()
    {
        // Note:
        // This test is not a real unit test.
        // It is a helper that provides an easy way to get the appropriate triples from a turtle file definition.

        using var graph = new Graph();
        var turtleParser = new TurtleParser();
        var nTriplesWriter = new NTriplesWriter();

        turtleParser.Load(graph, "Models/TestData/turtleData.ttl");
        // Have a look at the generated triples :-)
        var nTriplesData = StringWriter.Write(graph, nTriplesWriter);
    }

    [TestCaseSource(nameof(_testCases))]
    public void GetTriples_ForCollections_ShouldReturnTriples(Collection collection, Entry entry, string expectedResult)
    {
        collection.Entries.Clear();
        collection.Entries.Add(entry);

        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = collection.GetTriples(graph, false);
        graph.Assert(triples);

        var writer = new NTriplesWriter();

        var nTriplesData = StringWriter.Write(graph, writer);
        var expectedData = File.ReadAllText(expectedResult);

        Assert.That(nTriplesData, Is.EqualTo(expectedData));
    }

    [Test]
    public void GetTriples_WithAddCubeLinkData_ShouldReturnTriplesWithCubeLinkAdditions()
    {
        _collectionPFS17.Entries.Clear();
        _collectionPFS17.Entries.Add(_entry109117);

        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = _collectionPFS17.GetTriples(graph, true);
        graph.Assert(triples);

        var writer = new NTriplesWriter();

        var nTriplesData = StringWriter.Write(graph, writer);
        var expectedData = File.ReadAllText("Models/TestData/109117CubeLinkData");

        Assert.That(nTriplesData, Is.EqualTo(expectedData));
    }

    private static readonly object[] _testCases =
    {
        new object[] { _collectionPFS17, _entry109123, "Models/TestData/109123" },
        new object[] { _collectionPFS17, _entry109117, "Models/TestData/109117" },
        new object[] { _collectionMET16, _entry388009, "Models/TestData/388009" }
    };
}
