namespace Bk.Termdat.Business.Resource.Models;

public class AdminInfoUpdate : IAdminInfoModification
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}
