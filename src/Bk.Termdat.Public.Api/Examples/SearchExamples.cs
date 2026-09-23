using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class SearchExamples : IMultipleExamplesProvider<IEnumerable<EntryDetail>>
{
    public IEnumerable<SwaggerExample<IEnumerable<EntryDetail>>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Search with Details",
            "CollectionIds=1109&InLanguageCode=DE&OutLanguageCode=FR&SearchTerm=Bund&ResultType=Detail",
            new[]
            {
                new EntryDetail
                {
                    Id = 109472,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109472?s=EntryId:109472",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1109, Code = "POG14", Text = "Geo-politische Benennungen (Staaten)"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 12010, Code = "POLI", Text = "POLITIK"
                    },
                    Subject = new List<SubjectSummary>
                    {
                        new()
                        {
                            Id = 19,
                            Text = "GEOWISSENSCHAFTEN",
                            Domains = new List<DomainSummary>
                            {
                                new()
                                {
                                    Id = 142,
                                    Text = "Geografie"
                                }
                            }
                        },
                        new()
                        {
                            Id = 30,
                            Text = "RECHT",
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 373448,
                            LanguageIsoCode = "DE",
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
                            NoteSource = "BK, Sektion Terminologie, 2014",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Type = "<leer>",
                                    Text = "alliance F, Bund Schweizerischer Frauenorganisationen",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/11451?s=EntryId:11451"
                                }
                            }
                        },
                        new()
                        {
                            Id = 373456,
                            LanguageIsoCode = "DE",
                            Sequence = 2,
                            Name = "Schweiz",
                            Note = "USG: Kurzform",
                            NameSource = "Bundesverfassung, Art. 24 Abs. 2 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 373457,
                            LanguageIsoCode = "DE",
                            Sequence = 3,
                            Name = "Bund",
                            Note =
                                "USG: häufig; \"Bund\" bezeichnet weniger den Staat als vielmehr die Eidgenossenschaft (eidgenössisch) im Gegensatz zu den Kantonen (kantonal) und ebenso die Gesamtheit der Organe der Eidgenossenschaft (Parlament, Regierung, Verwaltung, Gerichte); [C] Bern; [M] Schweizer Franken",
                            NameSource = "Bundesverfassung, Art. 3 (SR 101, Stand 2014-01); EDA-DV, Dossier \"Staatenbezeichnungen\", 2013",
                            NoteSource = "BK, Sektion Terminologie, 2014"
                        },
                        new()
                        {
                            Id = 370865,
                            LanguageIsoCode = "FR",
                            Sequence = 1,
                            Abbreviation = "CH; CHE",
                            AbbreviationSource = "UNTERM ([Internet, 2021-06-10](https://unterm.un.org/unterm/country))",
                            Definition = "État fédératif démocratique, composé de 26 cantons, possédant quatre langues nationales (l'allemand, le français, l'italien et le romanche) et situé dans le sud de l'Europe centrale entre l'Allemagne, la France, le Liechtenstein, l'Autriche et l'Italie.",
                            DefinitionSource = "d'après Constitution fédérale, préambule, art. 1 et art. 4 (RS 101, état 2021-03)",
                            Name = "Confédération suisse",
                            NameSource = "Constitution fédérale, art. 1 (RS 101, état 2021-03)",
                            Note = "USG: dénomination officielle; ADJ: suisse, suissesse; jamais \"helvétique\", éviter si possible les adjectifs composés commençant par \"helveto-\"",
                            NoteSource = "d'après DFAE-DDIP, Liste des dénominations d'États, 2021",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Type = "<leer>",
                                    Text = "alliance F, Alliance de sociétés féminines suisses",
                                    Url = $"{Constants.TermdatBaseUrl}/search/entry/11451?s=EntryId:11451"
                                }
                            }
                        },
                        new()
                        {
                            Id = 370874,
                            LanguageIsoCode = "FR",
                            Sequence = 2,
                            Name = "Suisse",
                            NameSource = "Constitution fédérale, art. 24 al. 2 (RS 101, état 2021-03); DFAE-DDIP, Liste des dénominations d'États, 2021",
                            Note = "USG: forme courte; GRM: f.",
                            NoteSource = "DFAE-DDIP, Liste des dénominations d'États, 2021"
                        },
                        new()
                        {
                            Id = 370875,
                            LanguageIsoCode = "FR",
                            Sequence = 3,
                            Name = "Confédération",
                            NameSource = "Constitution fédérale, art. 3 (RS 101, état 2021-03)",
                            Note = "USG: \"Confédération helvétique\" incorrect",
                            NoteSource = "ChF, Section de terminologie, 2021"
                        }
                    }
                },
                new EntryDetail
                {
                    Id = 103562,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/103562?s=EntryId:103562",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 11451, Code = "ORGCH19", Text = "Bezeichnungen schweizerischer Organisationen und Institutionen"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11472, Code = "NATI", Text = "NATIONALE ORGANISATIONEN"
                    },
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
                                    Id = 13, Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13, Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493, Text = "Fachgebiet 2"
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 341323,
                            Sequence = 1,
                            LanguageIsoCode = "DE",
                            Metadata = "BSF, Bund Schweizerischer Frauenvereine",
                            Name = "alliance F, Bund Schweizerischer Frauenorganisationen",
                            NameSource = "alliance F, Statuten, 1948 (Stand 2016-05), Art. 1 ([Internet, 2020-01-17](https://de.alliancef.ch/ueber-uns#organisation))",
                            Note = "USG: offiziell; EXP: 1900 gegründeter Dachverband, der die Interessen der Frauen in der Schweizer Politik vertritt und die Gleichstellung von Frau und Mann in Wirtschaft, Beruf, Familie und Gesellschaft zum Ziel hat; HIS: 2001 wurde die Abkürzung \"BSF\" durch die Kurzbezeichnung \"alliance F\" ersetzt; 1900-1970 Bund Schweizerischer Frauenvereine",
                            NoteSource = "nach alliance F, Über uns ([Internet, 2020-01-17](https://de.alliancef.ch/ueber-uns))"
                        },
                        new()
                        {
                            Id = 341326,
                            Sequence = 2,
                            LanguageIsoCode = "DE",
                            Name = "alliance F",
                            NameSource = "alliance F, Statuten, 1948 (Stand 2016-05), Art. 2 ([Internet, 2020-01-17](https://de.alliancef.ch/ueber-uns#organisation))",
                            Note = "USG: offizielle Kurzform"
                        },
                        new()
                        {
                            Id = 341325,
                            Sequence = 1,
                            LanguageIsoCode = "FR",
                            Metadata = "Association suisse des organisations de femmes",
                            Name = "alliance F, Alliance de sociétés féminines suisses",
                            NameSource = "alliance F, Statuts, 1948 (état 2016-05), art. 1 ([Internet, 2020-01-17](https://fr.alliancef.ch/a-propos-de-nous#organisation))",
                            Note = "USG: officiel; variante non officielle; Association suisse des organisations de femmes; EXP: organisation faîtière, fondée en 1900, qui représente les intérêts des femmes en politique suisse et a pour but de parvenir à l'égalité entre les femmes et les hommes dans les affaires, le travail, la famille et la société; HIS: en 2001, l'abréviation \"ASF\" a été remplacée par \"alliance F\"",
                            NoteSource = "d'après alliance F, A propos de nous ([Internet, 2020-01-17](https://fr.alliancef.ch/a-propos-de-nous))"
                        },
                        new()
                        {
                            Id = 341329,
                            Sequence = 2,
                            LanguageIsoCode = "FR",
                            Name = "alliance F",
                            NameSource = "alliance F, Statuts, 1948 (état 2016-05), art. 2 ([Internet, 2020-01-17](https://fr.alliancef.ch/a-propos-de-nous#organisation))",
                            Note = "USG: forme abrégée officielle"
                        }
                    }
                },
                new EntryDetail
                {
                    Id = 481022,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/481022?s=EntryId:481022",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 431, Code = "DURAB21", Text = "Terminologie der nachhaltigen Entwicklung"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11478, Code = "UMWE", Text = "UMWELT"
                    },
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
                                    Id = 13, Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13, Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493, Text = "Fachgebiet 2"
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 1162644,
                            Sequence = 1,
                            LanguageIsoCode = "DE",
                            Abbreviation = "G BNE",
                            AbbreviationSource = "éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/de/weitere-netzwerke))",
                            Metadata = "Gruppe BNE-Bund, Bildung für nachhaltige Entwicklung",
                            Name = "Gruppe BNE Bund",
                            NameSource = "éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/de/weitere-netzwerke))",
                            Note = "USG: offiziell; EXP: interdepartementales Koordinationsorgan des Bundes im Bereich der Bildung für nachhaltige Entwicklung (BNE); darin sind folgende Bundesstellen vertreten: ARE (Leitung), BAFU, BFE, BAG, GS-EDI, SBFI, BLV, DEZA",
                            NoteSource = "nach éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/de/weitere-netzwerke)) und SBFI, Nachhaltige Entwicklung im BFI-Bereich, Übersicht über Aktivitäten mit Schwerpunkt Nachhaltige Entwicklung, 2020-04-20, S. 13 ([Internet, 2021-02-05](https://www.sbfi.admin.ch/dam/sbfi/de/dokumente/2020/02/uebersicht-nachhaltig.pdf.download.pdf/Nachhaltigkeit_Aktivitaeten_im_BFI_Bereich_de.pdf))",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Text = "Bildung für nachhaltige Entwicklung", Url = $"{Constants.TermdatBaseUrl}/search/entry/51594?s=EntryId:51594", Type = "<leer>"
                                },
                                new()
                                {
                                    Text = "Schweizerische Koordinationskonferenz Bildung für eine Nachhaltige Entwicklung", Url = $"{Constants.TermdatBaseUrl}/search/entry/382295?s=EntryId:382295", Type = "<leer>"
                                }
                            }
                        },
                        new()
                        {
                            Id = 1162645,
                            Sequence = 1,
                            LanguageIsoCode = "FR",
                            Abbreviation = "G EDD",
                            AbbreviationSource = "éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/fr/autres-reseaux))",
                            Name = "Groupe EDD de la Confédération",
                            NameSource = "éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/fr/autres-reseaux))",
                            Note = "USG: officiel; variante non officielle: Groupe EDD Confédération; EXP: organe de coordination interdépartemental de la Confédération chargé d'encourager l'éducation au développement durable; il est composé des représentants des services fédéraux suivants : ARE (direction), OFEV, OFEN, OFSP, SG-DFI, SEFRI, OSAV, DDC",
                            NoteSource = "d'après éducation21, Akteure und Netzwerke > Weitere Akteure und Netzwerke BNE in der Schweiz ([Internet, 2021-02-05](https://www.education21.ch/fr/autres-reseaux)) et SEFRI, Développement durable dans le domaine FRI\nVue d'ensemble des activités centrées sur le développement durable, 2020-04-20, p. 13 ([Internet, 2021-02-05](https://www.sbfi.admin.ch/dam/sbfi/fr/dokumente/2020/02/uebersicht-nachhaltig.pdf.download.pdf/Nachhaltigkeit_Aktivitaeten_im_BFI_Bereich_fr.pdf))",
                            References = new List<EntryReferenceSummary>
                            {
                                new()
                                {
                                    Text = "éducation au développement durable", Url = $"{Constants.TermdatBaseUrl}/search/entry/103562?s=EntryId:103562", Type = "<vide>"
                                },
                                new()
                                {
                                    Text = "Conférence suisse de coordination pour l’éducation au développement durable", Url = $"{Constants.TermdatBaseUrl}/search/entry/103562?s=EntryId:103562", Type = "<vide>"
                                }
                            }
                        }
                    }
                }
            }.AsEnumerable()
        );

        yield return SwaggerExample.Create(
            "Search with Summary",
            "CollectionIds=1109&InLanguageCode=DE&OutLanguageCode=FR&SearchTerm=Bund&ResultType=Summary",
            new[]
            {
                new EntryDetail
                {
                    Id = 109472,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109472?s=EntryId:109472",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1109, Code = "POG14", Text = "Geo-politische Benennungen (Staaten)"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 12010, Code = "POLI", Text = "POLITIK"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new() {{ "name", "Schweizerische Eidgenossenschaft" }},
                        new() {{ "abbreviation", "CH; CHE" }}
                    }
                },
                new EntryDetail
                {
                    Id = 103562,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/103562?s=EntryId:103562",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 11451, Code = "ORGCH19", Text = "Bezeichnungen schweizerischer Organisationen und Institutionen"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11472, Code = "NATI", Text = "NATIONALE ORGANISATIONEN"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new() { {"name", "Migros-Genossenschafts-Bund" } }
                    }
                },
                new EntryDetail
                {
                    Id = 481022,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/481022?s=EntryId:481022",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 431, Code = "DURAB21", Text = "Terminologie der nachhaltigen Entwicklung"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11478, Code = "UMWE", Text = "UMWELT"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new() { { "name", "Plattform Bildung für Nachhaltige Entwicklung EDK-Bund" } }
                    }
                }
            }.AsEnumerable()
        );

        yield return SwaggerExample.Create(
            "Search-2 with Details",
            "InLanguageCode=DE&OutLanguageCode=IT&SearchTerm=Schweiz*&Field.Terminus=true&ResultType=Detail",
            new[]
            {
                new EntryDetail
                {
                    Id = 109744,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109744?s=EntryId:109744",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 10382, Code = "POR19", Text = "Terminologie der politischen Rechte"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "RECH", Text = "RECHT"
                    },
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
                                    Id = 13, Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13, Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493, Text = "Fachgebiet 2"
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 372416,
                            LanguageIsoCode = "DE",
                            Sequence = 1,
                            Terminus = "Schweizervolk",
                            TerminusSource = "Bundesverfassung, Präambel (SR 101, Stand 2017-02)",
                        },
                        new()
                        {
                            Id = 372415,
                            LanguageIsoCode = "DE",
                            Sequence = 1,
                            Terminus = "schweizerisches Volk",
                            TerminusSource = "Bundesverfassung, Art. 72 Abs. 1 (alte Fassung, SR 101 Stand 1998)"
                        },
                        new()
                        {
                            Id = 498144,
                            Sequence = 1,
                            LanguageIsoCode = "IT",
                            Terminus = "popolo svizzero",
                            TerminusSource = "Costituzione federale, preambolo (RS 101, stato 2017-02)",

                        }
                    }
                },
                new EntryDetail
                {
                    Id = 133697,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/133697?s=EntryId:133697",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1258, Code = "SPRA20", Text = "Terminologie des Sprachenrechts"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "VARI", Text = "VARIA"
                    },
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
                                    Id = 13, Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13, Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493, Text = "Fachgebiet 2"
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 498135,
                            LanguageIsoCode = "DE",
                            Sequence = 1,
                            Definition = "Umgangssprache der deutschsprachigen Bevölkerung der Schweiz, die zahlreiche Dialekte umfasst.",
                            DefinitionSource = "nach Duden, Wie sagt man in der Schweiz?, 1989, \"Schweizerdeutsch\"",
                            Note = "EXP: es gibt keine standardisierte Schriftform des Schweizerdeutschen",
                            NoteSource = "BK, Sektion Deutsch, 1998",
                            Terminus = "Schweizerdeutsch",
                            TerminusSource = "Duden, \"Schweizerdeutsch\" ([Internet, 2020-07-22](https://www.duden.de/suchen/dudenonline/Schweizerdeutsch))",
                        },
                        new()
                        {
                            Id = 498144,
                            Sequence = 1,
                            LanguageIsoCode = "IT",
                            Definition = "Lingua corrente della popolazione germanofona in Svizzera, che comprende più dialetti.",
                            DefinitionSource = "secondo DFI, Quadrilinguismo svizzero, 1989, pag. 17 e Duden, Wie sagt man in der Schweiz?, 1989, \"Schweizerdeutsch\"",
                            Terminus = "svizzero-tedesco",
                            TerminusSource = "DFI, Quadrilinguismo svizzero, 1989, pag. 17",
                        },
                        new()
                        {
                            Id = 498140,
                            Sequence = 2,
                            LanguageIsoCode = "IT",
                            Terminus = "svizzero tedesco",
                            TerminusSource = "DFI, Quadrilinguismo svizzero, 1989, pag. 7",
                        }
                    }
                },
                new EntryDetail
                {
                    Id = 133698,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/133698?s=EntryId:133698",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1258, Code = "SPRA20", Text = "Terminologie des Sprachenrechts"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "VARI", Text = "VARIA"
                    },
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
                                    Id = 13, Text = "Sachgebiet 1"
                                },
                                new()
                                {
                                    Id = 13, Text = "Sachgebiet 2"
                                }
                            }
                        },
                        new()
                        {
                            Id = 13493, Text = "Fachgebiet 2"
                        }
                    },
                    LanguageDetails = new List<EntryLanguageDetail>
                    {
                        new()
                        {
                            Id = 498142,
                            LanguageIsoCode = "DE",
                            Sequence = 1,
                            Definition = "In der Deutschschweiz verwendete Varietät der deutschen Standardsprache.",
                            DefinitionSource = "nach Duden, Wie sagt man in der Schweiz?, 1989, \"Schweizerhochdeutsch\"",
                            Metadata = "Schweizer Hochdeutsch",
                            Terminus = "Schweizerhochdeutsch",
                            TerminusSource = "Duden, Wie sagt man in der Schweiz?, 1989, \"Schweizerhochdeutsch\""
                        },
                        new()
                        {
                            Id = 498144,
                            Sequence = 1,
                            LanguageIsoCode = "IT",
                            Definition = "Varietà del tedesco standard utilizzata nella Svizzera tedesca.",
                            DefinitionSource = "secondo Duden, Wie sagt man in der Schweiz?, 1989, \"Schweizerhochdeutsch\""

                        }
                    }
                },
            }.AsEnumerable()
        );

        yield return SwaggerExample.Create(
            "Search-2 with Summary",
            "InLanguageCode=DE&OutLanguageCode=IT&SearchTerm=Schweiz*&Field.Terminus=true&ResultType=Summary",
            new[]
            {
                new EntryDetail
                {
                    Id = 109744,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/109744?s=EntryId:109744",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 10382, Code = "POR19", Text = "Terminologie der politischen Rechte"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "RECH", Text = "RECHT"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new () { { "terminus", "Schweizervolk" } },
                        new () { { "terminus", "schweizerisches Volk" } },
                    }
                },
                new EntryDetail
                {
                    Id = 133697,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/133697?s=EntryId:133697",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1258, Code = "SPRA20", Text = "Terminologie des Sprachenrechts"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "VARI", Text = "VARIA"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new() { { "terminus", "Schweizerdeutsch" } }
                    }
                },
                new EntryDetail
                {
                    Id = 133698,
                    Url = $"{Constants.TermdatBaseUrl}/search/entry/133698?s=EntryId:133698",
                    Office = new OfficeSummary
                    {
                        Id = 1, Code = "ACH", Text = "Schweizerische Bundesverwaltung"
                    },
                    Collection = new CollectionSummary
                    {
                        Id = 1258, Code = "SPRA20", Text = "Terminologie des Sprachenrechts"
                    },
                    Reliability = new CodeAndText
                    {
                        Code = "3", Text = "Sprachlich/formal überprüft"
                    },
                    Status = new CodeAndText
                    {
                        Code = "1", Text = "validiert"
                    },
                    Classification = new ClassificationSummary
                    {
                        Id = 11479, Code = "VARI", Text = "VARIA"
                    },
                    Hits = new List<Dictionary<string, string>>
                    {
                        new() { { "terminus", "Schweizerhochdeutsch" } }
                    }
                },
            }.AsEnumerable()
        );
    }
}
