namespace Bk.Termdat.Gateway.Models;

public class TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public string? TokenType { get; init; }
    public int? ExpiresIn { get; init; }
}
