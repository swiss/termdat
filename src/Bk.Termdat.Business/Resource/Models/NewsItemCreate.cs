using System;

namespace Bk.Termdat.Business.Resource.Models;

public class NewsItemCreate : INewsItemModification
{
    public bool IsEnabled { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public required string TitleEn { get; set; }
    public required string TitleDe { get; set; }
    public required string TitleFr { get; set; }
    public required string TitleIt { get; set; }
    public required string TitleRm { get; set; }

    public required string TextEn { get; set; }
    public required string TextDe { get; set; }
    public required string TextFr { get; set; }
    public required string TextIt { get; set; }
    public required string TextRm { get; set; }
}

