using Bk.Termdat.CrossCutting.Configuration;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Infrastructure.Search.Tests;

public class TestSearchGateway : SearchGateway
{
    public TestSearchGateway(ILogger<SearchGateway> logger, IOptions<SearchOptions> searchOptions, string indexName = "termdat_index")
        : base(logger, searchOptions, indexName)
    {
    }

    protected override void InitializeIndex(string indexName)
    {
        Logger.LogInformation("Creating In-Memory Index for Testing");

        Directory = new RAMDirectory();
        Writer = new IndexWriter(Directory, IndexWriterConfig);
        SearcherManager = new SearcherManager(Writer, true, null);
    }
}
