namespace Api.Contracts;

public sealed record SaveProductRequest(
    string Sku,
    string Name,
    string Unit,
    int Quantity,
    string? Note = null)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Sku))
            errors[nameof(Sku)] = ["Артикул обов'язковий."];

        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 200)
            errors[nameof(Name)] = ["Назва обов'язкова, до 200 символів."];

        if (string.IsNullOrWhiteSpace(Unit) || Unit.Length > 20)
            errors[nameof(Unit)] =
                ["Одиниця виміру обов'язкова, до 20 символів."];

        if (Quantity < 0)
            errors[nameof(Quantity)] =
                ["Кількість не може бути від'ємною."];

        return errors.Count == 0;
    }
}