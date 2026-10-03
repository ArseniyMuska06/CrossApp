using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore inner)
    : ICatalogStore
{
    private readonly ICatalogStore _inner =
        inner ?? throw new ArgumentNullException(nameof(inner));

    private IReadOnlyList<Product>? _cachedList;

    private readonly Dictionary<string, Product?> _cachedById =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> List()
    {
        if (_cachedList is null)
            _cachedList = _inner.List();

        return _cachedList;
    }

    public Product? GetById(string id)
    {
        if (_cachedById.TryGetValue(id, out var cached))
            return cached;

        var product = _inner.GetById(id);
        _cachedById[id] = product;

        return product;
    }

    public void Add(Product item)
    {
        _inner.Add(item);
        ClearCache();
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        ClearCache();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);

        if (removed)
            ClearCache();

        return removed;
    }

    private void ClearCache()
    {
        _cachedList = null;
        _cachedById.Clear();
    }
}