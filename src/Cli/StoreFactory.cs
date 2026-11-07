using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args, string catalogPath)
    {
        bool useFile = args.Contains("--file");

        ICatalogStore innerStore = useFile
            ? new FileCatalogStore(catalogPath)
            : new InMemoryCatalogStore(SampleData.Products());

        return new CachingCatalogStore(innerStore);
    }
}