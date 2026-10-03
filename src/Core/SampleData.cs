using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> Products()
    {
        return new Product[]
        {
            Product.Create("P-001", "SKU-001", "Цемент М400 25 кг", "шт", 100),
            Product.Create("P-002", "SKU-002", "Пісок", "т", 20),
            Product.Create("P-003", "SKU-003", "Цегла", "шт", 1500),
            Product.Create("P-004", "SKU-004", "Щебінь", "т", 15),
            Product.Create("P-005", "SKU-005", "Гіпсова штукатурка", "шт", 60),
            Product.Create("P-006", "SKU-006", "Шпаклівка", "шт", 45),
            Product.Create("P-007", "SKU-007", "Ґрунтовка", "л", 80),
            Product.Create("P-008", "SKU-008", "Фарба біла", "л", 50),
            Product.Create("P-009", "SKU-009", "Гіпсокартон", "шт", 120),
            Product.Create("P-010", "SKU-010", "Профіль металевий", "м", 300),
            Product.Create("P-011", "SKU-011", "Саморізи", "шт", 5000),
            Product.Create("P-012", "SKU-012", "Дюбелі", "шт", 2000),
            Product.Create("P-013", "SKU-013", "Плитковий клей", "шт", 70),
            Product.Create("P-014", "SKU-014", "Монтажна піна", "шт", 35),
            Product.Create("P-015", "SKU-015", "Утеплювач", "м²", 200)
        };
    }
}