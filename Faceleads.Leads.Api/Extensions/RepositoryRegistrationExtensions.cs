using System.Reflection;

namespace Faceleads.Leads.Api.Extensions;

public static class RepositoryRegistrationExtensions
{
    /// <summary>
    /// Registers implementations that end with "Repository" by mapping them to their corresponding interface
    /// (e.g. UsuarioRepository -> IUsuarioRepository) when the interface exists. Otherwise registers the
    /// concrete type as scoped.
    /// </summary>
    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided", nameof(assemblies));
        }

        var repoTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository", StringComparison.Ordinal));

        foreach (var impl in repoTypes)
        {
            var interfaces = impl.GetInterfaces();

            // Prefer interface named I{ImplementationName} (conventional mapping)
            var preferred = interfaces.FirstOrDefault(i => i.Name == "I" + impl.Name);

            if (preferred is not null)
            {
                services.AddScoped(preferred, impl);
            }
            else if (interfaces.Any())
            {
                // Register all implemented interfaces to the implementation
                foreach (var i in interfaces)
                {
                    services.AddScoped(i, impl);
                }
            }
            else
            {
                // No interface found, register concrete type
                services.AddScoped(impl);
            }
        }

        return services;
    }
}
