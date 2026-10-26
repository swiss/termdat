using System;
using System.Globalization;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class TermdatUrlService : ITermdatUrlService
{
    private readonly TermdatClientOptions termdatClientOptions;

    public TermdatUrlService(IOptions<TermdatClientOptions> termdatClientOptions)
    {
        ArgumentNullException.ThrowIfNull(termdatClientOptions);

        this.termdatClientOptions = termdatClientOptions.Value;
    }

    public string GetTermdatUrl(int entryId)
    {
        var entryPath = string.Format(CultureInfo.InvariantCulture, termdatClientOptions.EntryDetailPath, entryId);
        var queryParams = string.Format(CultureInfo.InvariantCulture, termdatClientOptions.EntryIdSearchQueryParameter, entryId);

        var uriBuilder = new UriBuilder(termdatClientOptions.BaseUrl)
        {
            Path = entryPath,
            Query = queryParams,
        };

        return uriBuilder.Uri.ToString();
    }
}
