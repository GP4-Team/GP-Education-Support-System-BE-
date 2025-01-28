using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace ESS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddAutoMapper(assembly);

        // Use AssemblyReference instead of DependencyInjection
        services.AddValidatorsFromAssemblyContaining<AssemblyReference>();

        return services;
    }
}