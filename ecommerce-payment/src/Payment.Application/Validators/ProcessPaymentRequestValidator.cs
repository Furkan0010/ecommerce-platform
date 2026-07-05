using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Validators;

public class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().WithMessage("Sipariş numarası zorunludur.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
    }
}
