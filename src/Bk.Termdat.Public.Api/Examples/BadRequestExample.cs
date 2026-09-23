using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class BadRequestExample : IExamplesProvider<string>
{
    public string GetExamples()
    {
        return "{\r\n  \"type\": \"https://tools.ietf.org/html/rfc7231#section-6.5.1\",\r\n  \"title\": \"One or more validation errors occurred.\",\r\n  \"status\": 400,\r\n  \"traceId\": \"00-77eb5321ec40eaca1f74269215f4dedf-9feeb74d1531ead7-00\",\r\n  \"errors\": {\r\n    \"\": [\r\n      \"The provided language 'IDN' is not supported.\"\r\n    ]\r\n  }\r\n}";
    }
}
