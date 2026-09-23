using System.Threading.Tasks;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface IAnalyticsService
{
    Task TrackEvent(string category, string action, string? name = null);
}
