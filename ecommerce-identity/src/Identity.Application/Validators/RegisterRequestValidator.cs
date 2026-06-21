using FluentValidation;
using Identity.Application.DTOs;

namespace Identity.Application.Validators;

/// <summary>
/// BlogPlatform'daki gibi FluentValidation ile DTO doğrulaması.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola zorunludur.")
            .MinimumLength(6).WithMessage("Parola en az 6 karakter olmalıdır.");

        RuleFor(x => x.FullName)
            .MaximumLength(100);
    }
}
