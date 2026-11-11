using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Filters;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        // Отримати всі товари.
        group.MapGet("/", (
            ICatalogStore store,
            int? page,
            int? pageSize,
            string? search) =>
        {
            int currentPage = page ?? 1;
            int size = pageSize ?? 20;

            var errors = new Dictionary<string, string[]>();

            if (currentPage < 1)
                errors["page"] = ["Номер сторінки має бути не меншим за 1."];

            if (size < 1 || size > 100)
                errors["pageSize"] = ["Розмір сторінки має бути від 1 до 100."];

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            IEnumerable<Product> query = store.List();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();

                query = query.Where(product =>
                    product.Sku.Contains(
                        term, StringComparison.OrdinalIgnoreCase) ||
                    product.Name.Contains(
                        term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query
                .OrderBy(product => product.Id, StringComparer.Ordinal)
                .ToList();

            int total = filtered.Count;
            long offset = ((long)currentPage - 1) * size;

            var items = offset >= total
                ? new List<ProductDto>()
                : filtered
                    .Skip((int)offset)
                    .Take(size)
                    .Select(ToApiDto)
                    .ToList();

            return Results.Ok(new { items, total });
        });

        // Отримати товар за ID.
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, ICatalogStore store) =>
        {
            var product = store.GetById(id);

            return product is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(ToApiDto(product));
        });

        // Створити товар.
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem>
            (SaveProductRequest request, ICatalogStore store) =>
        {
            var product = Product.Create(
                Guid.NewGuid().ToString("N"),
                request.Sku,
                request.Name,
                request.Unit,
                request.Quantity,
                request.Note);

            store.Add(product);

            return TypedResults.Created(
                $"/api/products/{product.Id}",
                ToApiDto(product));
        })
        .AddEndpointFilter<SaveProductValidationFilter>();

        // Замінити дані товару.
        group.MapPut("/{id}",
            Results<Ok<ProductDto>, NotFound, ValidationProblem>
            (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            var existing = store.GetById(id);

            if (existing is null)
                return TypedResults.NotFound();

            var replacement = Product.Create(
                existing.Id,
                request.Sku,
                request.Name,
                request.Unit,
                request.Quantity,
                request.Note);

            store.Update(replacement);

            return TypedResults.Ok(ToApiDto(replacement));
        })
        .AddEndpointFilter<SaveProductValidationFilter>();

        // Видалити товар.
        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, ICatalogStore store) =>
                store.Remove(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound());
    }

    private static ProductDto ToApiDto(Product product) =>
        new(
            product.Id,
            product.Sku,
            product.Name,
            product.Unit,
            product.Quantity,
            product.Note);
}