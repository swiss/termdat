using Bk.Termdat.CrossCutting.Configuration;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Infrastructure.Search.Tests;

public class TestSearchGateway : SearchGateway
{
    public TestSearchGateway(ILogger<SearchGateway> logger, IOptions<SearchOptions> searchOptions, string indexName = "termdat_index")
        : base(logger, searchOptions, indexName, _ => new RAMDirectory())
    {
    }
}
