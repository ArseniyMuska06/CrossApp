using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Domain;
using Core.Reports;

namespace Cli;

public static class ReportBenchmark
{
    public static void Run()
    {
        const int itemCount = 100_000;
        const int threshold = 500;
        const int repetitions = 20;

        var random = new Random(7);
        var products = new List<Product>(itemCount);

        for (int i = 1; i <= itemCount; i++)
        {
            products.Add(Product.Create(
                $"P-{i:D6}",
                $"SKU-{i:D6}",
                $"Товар {i}",
                "шт",
                random.Next(0, 1000)));
        }

        // Прогрів обох варіантів перед вимірюванням.
        for (int i = 0; i < 3; i++)
        {
            ReportService.LowStock(products, threshold);
            ReportService.LowStockWithIntermediateList(
                products, threshold);
        }

        // Перевірка однаковості результатів поза вимірюванням.
        var first = ReportService.LowStock(products, threshold);
        var second = ReportService.LowStockWithIntermediateList(
            products, threshold);

        bool equal = first.Count == second.Count;

        for (int i = 0; equal && i < first.Count; i++)
        {
            equal = first[i] == second[i];
        }

        if (!equal)
        {
            Console.WriteLine("Помилка: результати звітів різні.");
            return;
        }

        double withoutListMs = 0;
        double withListMs = 0;
        var stopwatch = new Stopwatch();

        double Measure(Func<IReadOnlyList<StockLine>> report)
        {
            stopwatch.Restart();
            var result = report();
            stopwatch.Stop();

            GC.KeepAlive(result);
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        // Чергуємо порядок запусків обох варіантів.
        for (int i = 0; i < repetitions; i++)
        {
            if (i % 2 == 0)
            {
                withoutListMs += Measure(
                    () => ReportService.LowStock(
                        products, threshold));

                withListMs += Measure(
                    () => ReportService.LowStockWithIntermediateList(
                        products, threshold));
            }
            else
            {
                withListMs += Measure(
                    () => ReportService.LowStockWithIntermediateList(
                        products, threshold));

                withoutListMs += Measure(
                    () => ReportService.LowStock(
                        products, threshold));
            }
        }

        Console.WriteLine($"Згенеровано товарів: {products.Count}");
        Console.WriteLine($"Поріг залишку: {threshold}");
        Console.WriteLine($"Рядків у звіті: {first.Count}");
        Console.WriteLine($"Результати однакові: {equal}");
        Console.WriteLine($"Повторень кожного варіанта: {repetitions}");

        Console.WriteLine(
            $"Без проміжного ToList: " +
            $"{withoutListMs / repetitions:F3} мс");

        Console.WriteLine(
            $"З проміжним ToList: " +
            $"{withListMs / repetitions:F3} мс");
    }
}