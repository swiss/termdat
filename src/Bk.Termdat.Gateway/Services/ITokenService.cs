namespace Bk.Termdat.Gateway.Services;

public interface ITokenService
{
    Task<string?> GetValidAccessToken();
    Task SignOutUser();
}
