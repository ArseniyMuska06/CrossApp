using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfCatalogStore(AppDbContext db) : ICatalogStore
{
    public IReadOnlyList<Product> List()
    {
        return db.Products
            .AsNoTracking()
            .OrderBy(row => row.Sku)
            .AsEnumerable()
            .Select(ToDomain)
            .ToList();
    }

    public Product? GetById(string id)
    {
        var row = db.Products
            .AsNoTracking()
            .SingleOrDefault(row => row.Id == id);

        return row is null ? null : ToDomain(row);
    }

    public void Add(Product item)
    {
        var row = new ProductRow
        {
            Id = item.Id,
            Sku = item.Sku,
            Name = item.Name,
            Unit = item.Unit,
            Quantity = item.Quantity,
            Note = item.Note
        };

        db.Products.Add(row);
        db.SaveChanges();
    }

    public void Update(Product item)
    {
        var row = db.Products.Single(row => row.Id == item.Id);

        row.Sku = item.Sku;
        row.Name = item.Name;
        row.Unit = item.Unit;
        row.Quantity = item.Quantity;
        row.Note = item.Note;

        db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = db.Products.SingleOrDefault(row => row.Id == id);

        if (row is null)
            return false;

        db.Products.Remove(row);
        db.SaveChanges();
        return true;
    }

    private static Product ToDomain(ProductRow row)
    {
        return Product.Create(
            row.Id,
            row.Sku,
            row.Name,
            row.Unit,
            row.Quantity,
            row.Note);
    }
}