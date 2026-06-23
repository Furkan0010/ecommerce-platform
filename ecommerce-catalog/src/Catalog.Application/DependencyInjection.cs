using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // İş mantığı servisleri (kendi yazdığımız soyutlamalar).
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // FluentValidation validatörleri.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
