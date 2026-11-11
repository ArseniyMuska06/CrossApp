using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore(IEnumerable<Product>? seed = null)
    : ICatalogStore
{
    private readonly object _sync = new();

    private readonly Dictionary<string, Product> _items =
        (seed ?? []).ToDictionary(
            p => p.Id,
            StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> List()
    {
        lock (_sync)
        {
            return _items.Values.ToList();
        }
    }

    public Product? GetById(string id)
    {
        lock (_sync)
        {
            return _items.GetValueOrDefault(id);
        }
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException(
                    $"Запис з id={item.Id} уже існує.");

            _items.Add(item.Id, item);
        }
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            _items[item.Id] = item;
        }
    }

    public bool Remove(string id)
    {
        lock (_sync)
        {
            return _items.Remove(id);
        }
    }
}