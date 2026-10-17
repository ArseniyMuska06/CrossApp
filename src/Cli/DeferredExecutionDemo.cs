using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Cli;

public static class DeferredExecutionDemo
{
    public static void Run(IEnumerable<Product> items)
    {
        var source = items.ToList();

        // Запит створено, але фільтрація ще не виконувалася.
        var query = source.Where(p => p.Quantity < 10);

        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        source.Add(
            Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));

        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

        // Виконуємо запит і зберігаємо результат в окремому списку.
        var snapshot = query.ToList();

        source.Clear();

        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
    }
}