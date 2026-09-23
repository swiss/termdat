namespace Bk.Termdat.Business.Services.Interfaces;

public interface IIdentityService
{
    string GetCurrentEmail();
    string GetCurrentLastName();
    string GetCurrentFirstName();
    string GetCurrentRole();
}
