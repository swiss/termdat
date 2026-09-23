using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bk.Termdat.Common.Api.ModelBinder;

public class StringSanitizerModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        var value = valueProviderResult.FirstValue;

        var sanitized = string.IsNullOrWhiteSpace(value)
            ? null
            : Sanitize(value);

        bindingContext.Result = ModelBindingResult.Success(sanitized);

        return Task.CompletedTask;
    }

    private static string Sanitize(string value)
    {
        // Remove control-characters from our strings. We don't want to store them
        // We accept:
        //      - TAB
        //      - LF
        //      - CR
        //      - Space
        //      - ~
        //      - All other Unicode except DEL
        return Regex.Replace(value, @"[^\u0009\u000A\u000D\u0020-\u007E\u0080-\uffff]", string.Empty);
    }
}
