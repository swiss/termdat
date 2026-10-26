using System;
using System.Collections.Generic;

namespace Bk.Termdat.Business.Domain.Models;

public class UserAccount : EntityChangeTracking
{
    public string Email { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? Role { get; set; }

    public DateTime? LastAction { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? Information { get; set; }

    public ICollection<Glossary> AuthorizedGlossaries { get; set; }

    public ICollection<Collection> AuthorizedCollections { get; set; }
}
