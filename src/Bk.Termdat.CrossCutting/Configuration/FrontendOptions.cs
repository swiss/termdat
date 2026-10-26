#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class FrontendOptions
{
    public const string SectionKey = "Frontend";

    public string? Banner { get; init; }

    [Required]
    public required bool IsProduction { get; init; }

    [Required]
    public required string RequestAccessLink { get; init; }

    [Required]
    public required string MyAccount { get; init; }

    [Required]
    public required string AppUrl { get; init; }

    [Required]
    public required RoleOptions Roles { get; init; }

    [Required]
    public int DefaultOfficeId { get; init; }

    [Required]
    public int DefaultCollectionId { get; init; }

    [Required]
    public required string FeedbackEmail { get; init; }

    [Required]
    public required FrontendAnalyticsOptions Analytics { get; init; }

    [Required]
    public required string FroalaKey { get; init; }
}

public class RoleOptions
{
    [Required]
    public required string Allow { get; init; }

    [Required]
    public required string Admin { get; init; }

    [Required]
    public required string Writer { get; init; }

    [Required]
    public required string SuperUser { get; init; }

    [Required]
    public required string SuperReader { get; init; }
}

public class FrontendAnalyticsOptions
{
    [Required]
    public bool Enabled { get; init; }

    [Required]
    public required string TrackerUrl { get; init; }

    [Required]
    public required string ContainerUrl { get; init; }
}
