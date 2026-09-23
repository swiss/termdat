namespace Bk.Termdat.Business.Domain.Models;

public class EntryExternalLink
{
    public int Id { get; set; }

    public string Url { get; set; }

    public ExternalLinkType Type { get; set; }

    public int EntryId { get; set; }

    public Entry Entry { get; set; }
}
