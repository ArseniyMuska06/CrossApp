using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using System.Text.Encodings.Web;

namespace Core.Storage;

public sealed class FileCatalogStore(string path) : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Product> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(
                File.ReadAllText(_path), Options) ?? [];

            foreach (var dto in dtos)
            {
                var product = Product.FromDto(dto);
                _cache[product.Id] = product;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        var dtos = _cache.Values.Select(product => product.ToDto()).ToList();
        string json = JsonSerializer.Serialize(dtos, Options);
        string tempPath = _path + ".tmp";

        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _path, overwrite: true);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException(
                $"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Product item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}