namespace Bk.Termdat.Business.Domain.Models;

public class EntryTemplate : EntityChangeTracking
{
    public string Name { get; set; }
    public string Template { get; set; }
    public int UserAccountId { get; set; }

    public UserAccount UserAccount { get; set; }
}
