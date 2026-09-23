using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class GetEntriesExamples : IMultipleExamplesProvider<IEnumerable<EntryDetail>>
{
    public IEnumerable<SwaggerExample<IEnumerable<EntryDetail>>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "EntryDetail",
            "EntryIds=109472&InLanguageCode=de&OutLanguageCode=en",
            new[]
            {
                new EntryDetail
                {
                    Id = 109472,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109472?s=EntryId:109472",
                    Office = new() { Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung" },
                    Collection = new() { Id = 1109, Code = "POG14", Text = "Geo-politische Benennungen (Staaten)" },
                    Reliability = new() { Code = "3", Text = "Sprachlich/formal überprüft" },
                    Status = new() { Code = "1", Text = "validiert" },
                    Classification = new() {Id = 12010, Code = "POLI", Text = "POLITIK"},
                    Subject = new List<SubjectSummary>()
                    {
                        new()
                        {
                            Id = 13493,
                            Text = "Fachgebiet 1",
                            Domains = new List<DomainSummary>
                            {
                                new()
                                {
                                    Id = 13,
                                    Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13,
                                    Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493,
                            Text = "Fachgebiet 2",
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 373448,
                            LanguageIsoCode = "de",
                            Sequence = 1,
                            Name = "Schweizerische Eidgenossenschaft",
                            Abbreviation = "CH; CHE",
                            Definition =
                                "Aus 26 Kantonen bestehender demokratischer Bundesstaat mit vier Landessprachen (Deutsch, Französisch, Italienisch und Rätoromanisch), der am Südrand Mitteleuropas, zwischen Frankreich, Deutschland, Liechtenstein, Österreich und Italien gelegen ist.",
                            Note =
                                "USG: Vollform; \"Schweizerische Botschaft\"; [A] nie \"helvetisch\" und nie \"Schweizer\" als Adjektiv für offizielle Bezeichnungen, nur als Herkunftsbezeichnung wie in \"Schweizer Wein\"; [A] schweizerisch, eidgenössisch; [P] Schweizer, Schweizerin",
                            AbbreviationSource = "([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))",
                            DefinitionSource = "nach Bundesverfassung, Präambel, Art. 1 u. Art. 4 (SR 101, Stand 2014-01)",
                            NameSource = "Bundesverfassung, Art. 1 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373456,
                            LanguageIsoCode = "de",
                            Sequence = 2,
                            Name = "Schweiz",
                            Note = "USG: Kurzform",
                            NameSource = "Bundesverfassung, Art. 24 Abs. 2 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373457,
                            LanguageIsoCode = "de",
                            Sequence = 3,
                            Name = "Bund",
                            Note =
                                "USG: häufig; \"Bund\" bezeichnet weniger den Staat als vielmehr die Eidgenossenschaft (eidgenössisch) im Gegensatz zu den Kantonen (kantonal) und ebenso die Gesamtheit der Organe der Eidgenossenschaft (Parlament, Regierung, Verwaltung, Gerichte); [C] Bern; [M] Schweizer Franken",
                            NameSource = "Bundesverfassung, Art. 3 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373449,
                            LanguageIsoCode = "en",
                            Sequence = 1,
                            Name = "Swiss Confederation",
                            Abbreviation = "CH; CHE",
                            Definition =
                                "Democratic federal state which is formed by 26 cantons, has four national languages (German, French, Italian and Romansh) and is situated in southern central Europe between Germany, France, Liechtenstein, Austria and Italy.",
                            Note = "USG: official form; \"Embassy of Switzerland\"; [A] Swiss; [P] a Swiss",
                            AbbreviationSource = "([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))",
                            DefinitionSource = "after Switzerland's new Federal Constitution (transl.) 1999, preamble, art. 1, art. 4",
                            NameSource = "Switzerland's new Federal Constitution (transl.) 1999, art. 1; ([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))",
                            NoteSource = "([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))"
                        },
                        new()
                        {
                            Id = 373458,
                            LanguageIsoCode = "en",
                            Sequence = 2,
                            Name = "Switzerland",
                            Note = "USG: short form",
                            NameSource = "Switzerland's new Federal Constitution (transl.) 1999, art. 24 para. 2; ([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))",
                            NoteSource = "([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))"
                        },
                        new()
                        {
                            Id = 373459,
                            LanguageIsoCode = "en",
                            Sequence = 3,
                            Name = "Confederation",
                            Note = "USG: doesn't refer to Switzerland as a country, but is used to distinguish the federal state from its members; sometimes it is also used to describe all the organs of the federal state",
                            NameSource = "Switzerland's new Federal Constitution (transl.) 1999, art. 3",
                            NoteSource = "Federal Chancellery, Terminoloy Section, 2014"
                        }
                    }
                }
            }.AsEnumerable());

        yield return SwaggerExample.Create(
            "EntryDetail-2",
            "EntryIds=109472&InLanguageCode=de",
            new[]
            {
                new EntryDetail
                {
                    Id = 109472,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109472?s=EntryId:109472",
                    Office = new() { Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung" },
                    Collection = new() { Id = 1109, Code = "POG14", Text = "Geo-politische Benennungen (Staaten)" },
                    Reliability = new() { Code = "3", Text = "Sprachlich/formal überprüft" },
                    Status = new() { Code = "1", Text = "validiert" },
                    Classification = new() {Id = 12010, Code = "POLI", Text = "POLITIK"},
                    Subject = new List<SubjectSummary>
                    {
                        new()
                        {
                            Id = 13493,
                            Text = "Fachgebiet 1",
                            Domains = new List<DomainSummary>
                            {
                                new()
                                {
                                    Id = 13,
                                    Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13,
                                    Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493,
                            Text = "Fachgebiet 2",
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 373448,
                            LanguageIsoCode = "de",
                            Sequence = 1,
                            Name = "Schweizerische Eidgenossenschaft",
                            Abbreviation = "CH; CHE",
                            Definition =
                                "Aus 26 Kantonen bestehender demokratischer Bundesstaat mit vier Landessprachen (Deutsch, Französisch, Italienisch und Rätoromanisch), der am Südrand Mitteleuropas, zwischen Frankreich, Deutschland, Liechtenstein, Österreich und Italien gelegen ist.",
                            Note =
                                "USG: Vollform; \"Schweizerische Botschaft\"; [A] nie \"helvetisch\" und nie \"Schweizer\" als Adjektiv für offizielle Bezeichnungen, nur als Herkunftsbezeichnung wie in \"Schweizer Wein\"; [A] schweizerisch, eidgenössisch; [P] Schweizer, Schweizerin",
                            AbbreviationSource = "([UNTERM, 2014-01-03](http://unterm.un.org/DGAACS/unterm.nsf/WebView/FBEF6C8EB06C94D985256DC700440A1D?OpenDocument))",
                            DefinitionSource = "nach Bundesverfassung, Präambel, Art. 1 u. Art. 4 (SR 101, Stand 2014-01)",
                            NameSource = "Bundesverfassung, Art. 1 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373456,
                            LanguageIsoCode = "de",
                            Sequence = 2,
                            Name = "Schweiz",
                            Note = "USG: Kurzform",
                            NameSource = "Bundesverfassung, Art. 24 Abs. 2 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373457,
                            LanguageIsoCode = "de",
                            Sequence = 3,
                            Name = "Bund",
                            Note =
                                "USG: häufig; \"Bund\" bezeichnet weniger den Staat als vielmehr die Eidgenossenschaft (eidgenössisch) im Gegensatz zu den Kantonen (kantonal) und ebenso die Gesamtheit der Organe der Eidgenossenschaft (Parlament, Regierung, Verwaltung, Gerichte); [C] Bern; [M] Schweizer Franken",
                            NameSource = "Bundesverfassung, Art. 3 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                    }
                }
            }.AsEnumerable());

        yield return SwaggerExample.Create(
"EntryDetail-3",
"EntryIds=86778&EntryIds=109472&InLanguageCode=fr&OutLanguageCode=it",
new[]
{
                new EntryDetail
                {
                    Id = 86778,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/86778?s=EntryId:86778",
                    Office = new() { Id = 1, Code = "ACH", Text = "Administration fédérale suisse" },
                    Collection = new() { Id = 10299, Code = "SCHKG20", Text = "Terminologie de la poursuite pour dettes et faillite" },
                    Reliability = new() { Code = "4", Text = "Fiche contrôlée par les experts" },
                    Status = new() { Code = "1", Text = "validé" },
                    Classification = new() {Id = 11475, Code = "RECH", Text = "RECHT"},
                    Subject = new List<SubjectSummary>
                    {
                        new()
                        {
                            Id = 1,
                            Text = "DROIT PÉNAL",
                            Domains = new List<DomainSummary>
                            {
                                new()
                                {
                                    Id = 13,
                                    Text = "DROIT COMMERCIAL"
                                },
                                new()
                                {
                                    Id = 14,
                                    Text = "PROCÉDURE - JUSTICE"
                                }
                            }
                        },
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 288735,
                            LanguageIsoCode = "fr",
                            Sequence = 1,
                            Definition = "Mesure conservatoire officielle sur les biens du débiteur pour garantir une créance qui fait l’objet d’une poursuite pendante ou future.",
                            DefinitionSource = "d'après Gilliéron, Poursuite pour dettes, faillite et concordat, 2012, p. 502",
                            Note = "DOM: poursuite pour dettes et faillite; EXP: le séquestre est la seule mesure conservatoire que le droit fédéral permette de prendre pour garantir l'exécution forcée ayant pour objet une somme d'argent ou la prestation de sûretés",
                            NoteSource = "Gilliéron, Poursuite pour dettes, faillite et concordat, 2012, p. 504",
                            Terminus = "séquestre",
                            TerminusSource = "LF Poursuite pour dettes et faillite, art. 271 al. 1 (RS 281.1, état 2019-01)",
                        },
                        new()
                        {
                            Id = 288736,
                            LanguageIsoCode = "it",
                            Sequence = 1,
                            Definition = "Misura cautelare destinata alla conservazione di beni del debitore in vista della loro realizzazione in una procedura di esecuzione pendente o futura.",
                            DefinitionSource = "secondo Borghi/De Rossa, Compendio di diritto dell'economia, 2007, pag. 689",
                            Note = "DOM: esecuzione e fallimento; EXP: si applica ai beni che il creditore non può ancora far pignorare o inventariare poiché non ha ancora effettuato le formalità della procedura preliminare",
                            NoteSource = "secondo Borghi/De Rossa, Compendio di diritto dell'economia, 2007, pag. 689",
                            Terminus = "sequestro",
                            TerminusSource = "LF Esecuzione e fallimento, art. 271 cpv. 1 (RS 281.1, stato 2019-01)"
                        }
                    }
                },
                new EntryDetail
                {
                    Id = 108197,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/108197?s=EntryId:108197",
                    Office = new() { Id = 1, Code = "ACH", Text = "Administration fédérale suisse" },
                    Collection = new() { Id = 1088, Code = "PEN15", Text = "Terminologie du droit pénal suisse" },
                    Reliability = new() { Code = "4", Text = "Fiche contrôlée par les experts" },
                    Status = new() { Code = "1", Text = "validé" },
                    Classification = new() {Id = 11475, Code = "RECH", Text = "RECHT"},
                    Subject = new List<SubjectSummary>(),
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 363917,
                            LanguageIsoCode = "fr",
                            Sequence = 1,
                            Context = "Si l'auteur a agi dans un dessein de lucre, la peine sera une peine privative de liberté de trois ans au plus ou une peine pécuniaire.",
                            ContextSource = "Code pénal suisse, art. 135 al. 3 (RS 311.0, état 2014-07)",
                            Country = "CH",
                            Definition = "Mobile qui pousse une personne à rechercher un enrichissement particulièrement répréhensible du point de vue moral, en mettant en cause des valeurs relatives à ce qui fait la dignité de la personne et dont la caractéristique est de ne pas être monnayables ou d'être bafouées lorsqu'elles sont monnayées.",
                            DefinitionSource = "d'après Arrêts du Tribunal fédéral, 107 IV 119 (1981)",
                            Metadata = "agir dans un dessein de lucre",
                            MetadataSource = "Code pénal suisse, art. 135 al. 3 (RS 311.0, état 2014-07)",
                            Phraseology = "dans un dessein de lucre",
                            PhraseologySource = "Code pénal suisse, art. 135 al. 3 (RS 311.0, état 2014-07)",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Type = "<vide>",
                                    Text = "par cupidité",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/108198?s=EntryId:108198",
                                },
                                new()
                                {
                                    Type = "<vide>",
                                    Text = "sans dessein d'enrichissement",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/108202?s=EntryId:108202",
                                }
                            }
                        },
                        new()
                        {
                            Id = 363920,
                            LanguageIsoCode = "fr",
                            Sequence = 2,
                            Note = "USG: ancien droit",
                            Phraseology = "dans un but de lucre",
                            PhraseologySource = "Code pénal suisse, art. 159 al. 2 (RS 3 235)",
                        },
                        new()
                        {
                            Id = 363918,
                            LanguageIsoCode = "it",
                            Sequence = 1,
                            Context = "Se il colpevole ha agito per fine di lucro, la pena è una pena detentiva sino a tre anni o una pena pecuniaria.",
                            ContextSource = "Codice penale svizzero, art. 135 cpv. 3 (RS 311.0, stato 2014-07)",
                            Country = "CH",
                            Definition = "Motivo che spinge una persona a proporsi un arricchimento particolarmente riprensibile sul piano morale perchè pone in gioco valori inerenti alla dignità umana caratterizzati dal fatto di non essere valutabili in denaro o di essere vilipesi se valuti in denaro.",
                            DefinitionSource = "secondo Decisioni del Tribunale federale, 107 IV 119 (1981)",
                            Metadata = "agire per fine di lucro",
                            MetadataSource = "Codice penale svizzero, art. 135 cpv. 3 (RS 311.0, stato 2014-07)",
                            Note = "EXP: \"fine di lucro\" nel senso qualitativo (CP, parte speciale)",
                            NoteSource = "secondo Decisioni del Tribunale federale, 107 IV 119 (1981)",
                            Phraseology = "per fine di lucro",
                            PhraseologySource = "Codice penale svizzero, art. 135 cpv. 3 (RS 311.0, stato 2014-07)",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Type = "<vuoto>",
                                    Text = "per fine di lucro",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/108198?s=EntryId:108198",
                                },
                                new()
                                {
                                    Type = "<vuoto>",
                                    Text = "senza fine di lucro",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/108202?s=EntryId:108202",
                                }
                            }
                        },
                    }
                }
}.AsEnumerable());
    }
}
