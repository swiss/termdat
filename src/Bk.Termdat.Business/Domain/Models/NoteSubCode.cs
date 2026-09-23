using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class NoteSubCode : EntityChangeTracking, IMultiLingualEntity<NoteSubCodeTranslation>
{
    public string Code { get; set; }
    public ICollection<NoteSubCodeTranslation> Translations { get; set; }
    public ICollection<Entry> Entries { get; set; }
    public ICollection<UserAccount> UserAccounts { get; set; }
}
