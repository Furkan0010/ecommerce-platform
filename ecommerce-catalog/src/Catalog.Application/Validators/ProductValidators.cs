using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün adı zorunludur.").MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Geçerli bir kategori seçiniz.");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün adı zorunludur.").MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Geçerli bir kategori seçiniz.");
    }
}
