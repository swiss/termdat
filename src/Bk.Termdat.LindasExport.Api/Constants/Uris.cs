namespace Bk.Termdat.LindasExport.Api.Constants;

#pragma warning disable S1075 // URIs are standard RDF vocabulary identifiers
#pragma warning disable S5332 // HTTP URIs are required RDF vocabulary identifiers
public static class Uris
{
    public const string Type = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
    public const string Identifier = "http://schema.org/identifier";
    public const string Name = "http://schema.org/name";
    public const string Url = "http://schema.org/URL";
    public const string Description = "http://schema.org/description";
    public const string AlternateName = "http://schema.org/alternateName";
    public const string Citation = "http://schema.org/citation";
    public const string Position = "http://schema.org/position";
    public const string HasPart = "http://schema.org/hasPart";
    public const string IsPartOf = "http://schema.org/isPartOf";
    public const string DefinedTermSet = "http://schema.org/DefinedTermSet";
    public const string DefinedTerm = "http://schema.org/DefinedTerm";
    public const string Synonym = "https://schema.ld.admin.ch/Synonym";
    public const string TermType = "https://schema.ld.admin.ch/Term";
    public const string NameType = "https://schema.ld.admin.ch/Name";
    public const string PhraseologyType = "https://schema.ld.admin.ch/Phraseology";
    public const string AbbreviationType = "https://schema.ld.admin.ch/Abbreviation";
    public const string TerminologyOfficeType = "https://schema.ld.admin.ch/TerminologyOffice";
    public const string GlossaryType = "https://schema.ld.admin.ch/Glossary";
    public const string AdministrativeNoteType = "https://schema.ld.admin.ch/AdministrativeNote";
    public const string TerminologyCollectionType = "https://schema.ld.admin.ch/TerminologyCollection";
    public const string ValidatedEntryType = "https://schema.ld.admin.ch/ValidatedEntry";
    public const string InProgressType = "https://schema.ld.admin.ch/InProgressEntry";
    public const string ReliabilityCodeType = "https://schema.ld.admin.ch/reliabilityCode";
    public const string TermDomainType = "https://schema.ld.admin.ch/TermDomain";
    public const string TermDomain = "https://schema.ld.admin.ch/termDomain";
    public const string TermSubDomainType = "https://schema.ld.admin.ch/TermSubDomain";
    public const string LinguisticContextType = "https://schema.ld.admin.ch/linguisticContext";
    public const string TermNoteType = "https://schema.ld.admin.ch/termNote";
    public const string ApplicableCountryType = "https://schema.ld.admin.ch/applicableCountry";
    public const string DescriptionCitationType = "https://schema.ld.admin.ch/descriptionCitation";
    public const string TermNoteCitationType = " https://schema.ld.admin.ch/termNoteCitation";
    public const string InDefinedTermSet = "http://schema.org/inDefinedTermSet";
    public const string Deprecated = "https://version.link/Deprecated";
    public const string ReferenceNone = "http://www.w3.org/2000/01/rdf-schema#seeAlso";
    public const string ReferenceGeneral = "http://www.lexinfo.net/ontology/3.0/lexinfo#hypernym";
    public const string ReferenceSpecific = "http://www.lexinfo.net/ontology/3.0/lexinfo#hyponym";
    public const string ReferenceAntonym = "http://www.lexinfo.net/ontology/3.0/lexinfo#antonym";
    public const string ReferenceWhole = "http://www.lexinfo.net/ontology/3.0/lexinfo#memberMeronym";
    public const string ReferencePart = "http://www.lexinfo.net/ontology/3.0/lexinfo#memberHolonym";
    public const string ObservationType = "https://cube.link/Observation";
    public const string Observation = "https://cube.link/observation";
    public const string ObservationSet = "https://cube.link/ObservationSet";
    public const string ObservedBy = "https://cube.link/observedBy";
    public const string ObservedByObject = "https://ld.admin.ch/FCh";
    public const string TermdatGraph = "https://lindas.admin.ch/fch/termdat";
    public const string TermdatCollectionUriFormat = "https://register.ld.admin.ch/termdat/{0}";
    public const string TermdatEntryUriFormat = "https://register.ld.admin.ch/termdat/{0}";
    public const string TermdatEntryAdditionUriFormat = "https://register.ld.admin.ch/termdat/{0}/{1}";
    public const string TermdatEntryPartUriFormat = "https://register.ld.admin.ch/termdat/{0}/{1}/{2}";
    public const string TermdatEntryPartAdditionUriFormat = "https://register.ld.admin.ch/termdat/{0}/{1}/{2}/{3}";
    public const string TermdatLinkFormat = "https://www.termdat.bk.admin.ch/entry/{0}";
    public const string TermdatTermUriAddition = "term";
    public const string TermdatPhraseologyUriAddition = "phraseology";
    public const string TermdatAbbreviationUriAddition = "abbreviation";
    public const string TermdatAdministrativeNoteUriAddition = "administrative-note";
    public const string TermdatTerminologyOfficeUriAddition = "terminology-office";
    public const string TermdatGlossaryUriAddition = "glossary";
    public const string TermdatReliabilityCodeUriAddition = "reliability-code";
    public const string TermdatDomainUriAddition = "domain";
    public const string TermdatSubDomainUriAddition = "sub-domain";

    public const string DataSetUri = "https://register.ld.admin.ch/.well-known/dataset/termdat";
    public const string DataSetUriObservation = "https://register.ld.admin.ch/.well-known/dataset/termdat/1/observation";
    public const string RdfsDataSet = "http://rdfs.org/ns/void#Dataset";
    public const string W3DataSet = "http://www.w3.org/ns/dcat#Dataset";
    public const string DataSetSubject = "http://schema.org/Dataset";
    public const string DataSetPredicate = "http://schema.org/dataset";
    public const string LindasDataSet = "https://schema.ld.admin.ch/LindasDataset";
    public const string SparqlEndpoint = "http://rdfs.org/ns/void#sparqlEndpoint";
    public const string AdminQuery = "https://register.ld.admin.ch/query/";
    public const string ExampleResource = "http://rdfs.org/ns/void#exampleResource";
    public const string ExampleResourceUrl = "https://register.ld.admin.ch/termdat/56905";
    public const string TermdatUrl = "https://termdat.ch/";
    public const string Creator = "http://schema.org/creator";
    public const string CreatorUrl = "https://ld.admin.ch/ou/10010843";
    public const string Publisher = "http://schema.org/publisher";
    public const string Lindas = "https://ld.admin.ch/org/lindas";
    public const string ContactPoint = "http://schema.org/contactPoint";
    public const string ContactPointUrl = "https://ld.admin.ch/ou/10010843";
    public const string DateCreated = "http://schema.org/dateCreated";
    public const string Date = "http://www.w3.org/2001/XMLSchema#date";
    public const string DatePublished = "http://schema.org/datePublished";
    public const string DateModified = "http://schema.org/dateModified";
    public const string LandingPage = "http://www.w3.org/ns/dcat#landingPage";
    public const string LandingPageUrl = "https://www.termdat.bk.admin.ch/";
    public const string Access = "http://www.w3.org/ns/dcat#accessURL";
    public const string AccessUrl = "https://register.ld.admin.ch/sparql/";
    public const string Void = "https://register.ld.admin.ch/.well-known/void";
}
#pragma warning restore S5332
#pragma warning restore S1075
