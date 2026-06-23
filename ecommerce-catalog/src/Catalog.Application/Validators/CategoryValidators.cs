using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Kategori adı zorunludur.").MaximumLength(100);
    }
}
