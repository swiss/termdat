using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class InternalServerErrorExample : IExamplesProvider<string>
{
    public string GetExamples()
    {
        return "An internal server has occurred, try again later or contact support.";
    }
}
