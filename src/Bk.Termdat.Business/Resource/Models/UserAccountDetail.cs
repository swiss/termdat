using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Resource.Models;

public class UserAccountDetail
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public bool HasRole { get; set; }
    public string Role { get; set; }
    public bool HasFullAccess { get; set; }
    public bool IsWriter { get; set; }
    public bool IsSuperReader { get; set; }
    public DateTime? LastAction { get; set; }
    public string? Information { get; set; }
    public IList<GlossarySummary> Glossaries { get; set; }
    public IList<CollectionSummary> Collections { get; set; }
}
