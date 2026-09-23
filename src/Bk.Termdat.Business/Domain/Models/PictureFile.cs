namespace Bk.Termdat.Business.Domain.Models;

public class PictureFile
{
    public int EntryId { get; set; }

    public byte[] Value { get; set; }

    public Picture Picture { get; set; }
}
