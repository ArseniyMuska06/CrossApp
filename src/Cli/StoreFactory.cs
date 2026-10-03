using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");

        string dataPath = Path.Combine(
            AppContext.BaseDirectory, "data", "catalog.json");

        ICatalogStore innerStore = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        return new CachingCatalogStore(innerStore);
    }
}