using Basket.Application.DTOs;
using FluentValidation;

namespace Basket.Application.Validators;

public class AddItemRequestValidator : AbstractValidator<AddItemRequest>
{
    public AddItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Geçerli bir ürün seçiniz.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Adet 0'dan büyük olmalıdır.");
    }
}
