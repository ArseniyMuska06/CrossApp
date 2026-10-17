namespace Core.Reports;

public sealed record MovementSummary(
    string Sku,
    string Name,
    int MovementCount,
    int TotalDelta);