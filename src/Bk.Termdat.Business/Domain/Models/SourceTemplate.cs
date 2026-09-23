namespace Bk.Termdat.Business.Domain.Models;

public class SourceTemplate : EntityChangeTracking
{
    public string? TextDe { get; set; }
    public string? TextFr { get; set; }
    public string? TextIt { get; set; }
    public string? TextEn { get; set; }
    public string? TextRm { get; set; }
    public int UserId { get; set; }
    public UserAccount User { get; set; }
}
