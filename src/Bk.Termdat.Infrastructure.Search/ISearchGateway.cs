using System;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Bk.Termdat.Infrastructure.Search;

public interface ISearchGateway : IDisposable
{
    Analyzer DefaultAnalyzer { get; }
    IndexWriter Writer { get; }
    LuceneVersion LuceneVersion { get; }
    SearcherManager SearcherManager { get; }
    void MaybeRefreshSearcherManager();
}
