using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bk.Termdat.Common.Api.ModelBinder;

public class StringSanitizerModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(string))
        {
            return new StringSanitizerModelBinder();
        }

        return null;
    }
}
