using System;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class TermdatUrlService : ITermdatUrlService
{
    private readonly TermdatClientOptions termdatClientOptions;

    public TermdatUrlService(IOptions<TermdatClientOptions> termdatClientOptions)
    {
        this.termdatClientOptions = termdatClientOptions.Value;
    }

    public string GetTermdatUrl(int entryId)
    {
        var entryPath = string.Format(termdatClientOptions.EntryDetailPath, entryId);
        var queryParams = string.Format(termdatClientOptions.EntryIdSearchQueryParameter, entryId);

        var uriBuilder = new UriBuilder(termdatClientOptions.BaseUrl)
        {
            Path = entryPath,
            Query = queryParams,
        };

        return uriBuilder.Uri.ToString();
    }
}
