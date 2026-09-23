using System;

namespace Bk.Termdat.Business.Domain.Models;

public class EntityChangeTracking : Entity
{
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedBy { get; set; }
}
