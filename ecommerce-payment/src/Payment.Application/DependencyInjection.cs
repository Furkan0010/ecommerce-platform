using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Interfaces;
using Payment.Application.Services;

namespace Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
