using VDS.RDF.Storage;

namespace Bk.Termdat.LindasExport.Api.Interfaces;

public interface ISparqlClientFactory
{
    IReadOnlyDictionary<string, IAsyncStorageProvider> GetStorageProviders();
}
