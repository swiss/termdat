using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class Picture
{
    public string FileExtension { get; set; }

    public string ContentType { get; set; }

    public string EntityTag { get; set; }

    public int EntryId { get; set; }

    public PictureFile File { get; set; }

    public Entry Entry { get; set; }

    public ICollection<PictureTranslation> Translations { get; set; }
}
