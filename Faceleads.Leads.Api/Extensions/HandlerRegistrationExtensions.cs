using System.Reflection;

namespace Faceleads.Leads.Api.Extensions;

public static class HandlerRegistrationExtensions
{
    /// <summary>
    /// Registers all non-abstract classes whose name ends with "Handler" from the provided assemblies as scoped services.
    /// This helps avoid forgetting to register new handlers when they are added to the application project.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided", nameof(assemblies));
        }

        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler", StringComparison.Ordinal));

        foreach (var type in handlerTypes)
        {
            // Register concrete handler type as itself (consumers typically depend on concrete handlers)
            services.AddScoped(type);
        }

        return services;
    }
}
