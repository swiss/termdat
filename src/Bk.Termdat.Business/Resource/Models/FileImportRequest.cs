using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Resource.Models;

public class FileImportRequest
{
    [Required]
    public IFormFile File { get; init; }

    [Required]
    public int EntryOffice { get; set; }

    [Required]
    public int EntryCollection { get; set; }

    [Required]
    public IList<int> SubjectIds { get; set; }

    public IList<int> DomainIds { get; set; }

    public IList<int> GlossaryIds { get; set; }

    [Required]
    public Status Status { get; set; }

    [Required]
    public Reliability Reliability { get; set; }

    public string Comment { get; set; }

    public IList<AdminInfoSummary> AdminInfos { get; set; } = new List<AdminInfoSummary>();
}
