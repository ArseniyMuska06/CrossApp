namespace Api.Contracts;

public sealed record ProductDto(
    string Id,
    string Sku,
    string Name,
    string Unit,
    int Quantity,
    string? Note);