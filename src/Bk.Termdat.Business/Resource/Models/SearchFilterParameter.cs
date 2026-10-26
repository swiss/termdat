using System;

namespace Bk.Termdat.Business.Resource.Models;

public class SearchFilterParameter
{
    public string? Phrase { get; set; }
    public int[] SourceLanguageIds { get; set; } = [];
    public int[] EntryIds { get; set; } = [];
    public int[] Offices { get; set; } = [];
    public bool OfficesPriority { get; set; }
    public int[] LenochClassifications { get; set; } = [];
    public bool LenochClassificationPriority { get; set; }
    public int[] Collections { get; set; } = [];
    public bool CollectionsPriority { get; set; }
    public int[] RelatedCollections { get; set; } = [];
    public int[] Glossaries { get; set; } = [];
    public string[] SubjectDomains { get; set; } = [];
    public bool SubjectDomainsPriority { get; set; }
    public Status[] Status { get; set; } = [];
    public bool StatusPriority { get; set; }
    public int[] AdminInfos { get; set; } = [];
    public bool AdminInfosPriority { get; set; }
    public CommentStatus[] CommentStatus { get; set; } = [];
    public string? CreatedBy { get; set; }
    public bool CreatedByPriority { get; set; }
    public string? ModifiedBy { get; set; }
    public bool ModifiedByPriority { get; set; }
    public DateTime? ModifiedFrom { get; set; }
    public DateTime? ModifiedTo { get; set; }
    public Reliability[] Reliability { get; set; } = [];
    public bool ReliabilityPriority { get; set; }
    public bool CollectionsExclude { get; set; }
    public bool GlossariesExclude { get; set; }
    public bool AdminInfosExclude { get; set; }
    public bool CreatedByExclude { get; set; }
    public bool ModifiedByExclude { get; set; }
    public bool ModifiedPeriodExclude { get; set; }
    public bool Deleted { get; set; }
    public bool DeletedPriority { get; set; }
    public bool ModifiedPeriodPriority { get; set; }
    public bool HasMissingSource { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}