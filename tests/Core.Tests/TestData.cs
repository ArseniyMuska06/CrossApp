using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static IReadOnlyList<Product> Items() =>
        new List<Product>
        {
            Product.Create(
                "TEST-1", "SKU-SMALL", "Кабель", "м", 3),

            Product.Create(
                "TEST-2", "SKU-MID", "Гвинт", "шт", 7),

            Product.Create(
                "TEST-3", "SKU-BIG", "Цегла", "шт", 20)
        };
}