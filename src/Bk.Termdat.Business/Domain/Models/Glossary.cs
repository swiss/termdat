using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Glossary : EntityChangeTracking, IMultiLingualEntity<GlossaryTranslation>
{
    public string Code { get; set; }
    public string? Description { get; set; }
    public ICollection<GlossaryTranslation> Translations { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<UserAccount> UserAccounts { get; set; }
}
