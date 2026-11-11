using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveProductRequestValidator
    : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(request => request.Sku)
            .NotEmpty()
            .WithMessage("Артикул обов'язковий.");

        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Назва обов'язкова.")
            .MaximumLength(200)
            .WithMessage("Назва має містити не більше 200 символів.");

        RuleFor(request => request.Unit)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Одиниця виміру обов'язкова.")
            .MaximumLength(20)
            .WithMessage(
                "Одиниця виміру має містити не більше 20 символів.");

        RuleFor(request => request.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Кількість не може бути від'ємною.");
    }
}