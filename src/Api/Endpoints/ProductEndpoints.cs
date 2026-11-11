using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        // Отримати всі товари.
        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(
                store.List().Select(ToApiDto).ToList()));

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
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

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
        });

        // Замінити дані товару.
        group.MapPut("/{id}",
            Results<Ok<ProductDto>, NotFound, ValidationProblem>
            (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

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
        });

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