namespace Bk.Termdat.CrossCutting;

public static class Policy
{
    public const string RequireAdminPolicy = nameof(RequireAdminPolicy);
    public const string RequireWriterPolicy = nameof(RequireWriterPolicy);
    public const string RequireSuperUserPolicy = nameof(RequireSuperUserPolicy);
    public const string RequireAdminOrWriterPolicy = nameof(RequireAdminOrWriterPolicy);
    public const string RequireAdminOrWriterOrSuperUserPolicy = nameof(RequireAdminOrWriterOrSuperUserPolicy);
    public const string RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy = nameof(RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy);
    public const string RequireAdminOrSuperUserPolicy = nameof(RequireAdminOrSuperUserPolicy);
}
