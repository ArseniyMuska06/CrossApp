using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Extensions;

namespace Core.Reports;

public static class ReportService
{
    // Товари із залишком меншим за заданий поріг.
    public static IReadOnlyList<StockLine> LowStock(
        IEnumerable<Product> source,
        int threshold) =>
        source
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    public static IReadOnlyList<StockLine> TopByQuantity(
        IEnumerable<Product> source,
        int n) =>
        source
            .TopBy(p => p.Quantity, n, p => p.Sku)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // Підсумки за одиницями виміру.
    public static IReadOnlyList<GroupSummary> ByUnit(
        IEnumerable<Product> source) =>
        source
            .GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(
                g.Key,
                g.Count(),
                g.Sum(p => p.Quantity)))
            .OrderByDescending(s => s.TotalQuantity)
            .ToList();

    // Загальні підсумки; для порожнього джерела — нулі.
    public static (int Items, int Total, double Average, int Max) Totals(
        IEnumerable<Product> source)
    {
        var list = source.ToList();

        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (
                list.Count,
                list.Sum(p => p.Quantity),
                list.Average(p => p.Quantity),
                list.Max(p => p.Quantity)
            );
    }

    public static IReadOnlyList<MovementSummary> ByMovements(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements) =>
        products
            .GroupJoin(
                movements,
                p => p.Id,
                m => m.ProductId,
                (p, group) => new MovementSummary(
                    p.Sku,
                    p.Name,
                    group.Count(),
                    group.Sum(m => m.Delta)))
            .OrderBy(s => s.Sku, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<MovementSummary> TopByMovementCount(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements,
        int n) =>
        ByMovements(products, movements)
            .TopBy(row => row.MovementCount, n, row => row.Sku)
            .ToList();

    public static IReadOnlyList<GroupSummary> ByUnitQuery(
        IEnumerable<Product> source)
    {
        var query =
            from product in source
            group product by product.Unit.ToUpperInvariant() into g
            orderby g.Sum(p => p.Quantity) descending
            select new GroupSummary(
                g.First().Unit,
                g.Count(),
                g.Sum(p => p.Quantity));

        return query.ToList();
    }

    public static IReadOnlyList<StockLine> LowStockWithIntermediateList(
        IEnumerable<Product> source,
        int threshold) =>
        source
            .Where(p => p.Quantity < threshold)
            .ToList()
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
}