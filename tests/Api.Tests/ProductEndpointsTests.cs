using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public sealed class ProductEndpointsTests
{
    [Fact]
    public async Task CreateProduct_ThenGetByLocation_ReturnsCreatedProduct()
    {
        // Підготовка: окремий тестовий сервер і HTTP-клієнт.
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var request = new SaveProductRequest(
            Sku: "TEST-100",
            Name: "Тестовий кабель",
            Unit: "м",
            Quantity: 40,
            Note: "Інтеграційний тест");

        // Створення товару через HTTP.
        using var postResponse = await client.PostAsJsonAsync(
            "/api/products",
            request);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var location = postResponse.Headers.Location;
        Assert.NotNull(location);

        var created = await postResponse.Content
            .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
        Assert.Equal(
            $"/api/products/{created.Id}",
            location!.OriginalString);

        // Читання саме за адресою з Location.
        using var getResponse = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var loaded = await getResponse.Content
            .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded.Id);
        Assert.Equal(request.Sku, loaded.Sku);
        Assert.Equal(request.Name, loaded.Name);
        Assert.Equal(request.Unit, loaded.Unit);
        Assert.Equal(request.Quantity, loaded.Quantity);
        Assert.Equal(request.Note, loaded.Note);
    }
}