using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class NotFoundExample : IExamplesProvider<string>
{
    public string GetExamples()
    {
        return "The provided entry ids=[9999999,999998] are not found.";
    }
}
