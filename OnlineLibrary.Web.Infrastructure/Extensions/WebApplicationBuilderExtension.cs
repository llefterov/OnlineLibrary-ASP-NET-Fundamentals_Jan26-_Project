using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace OnlineLibrary.Web.Infrastructure.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection serviceCollection, Type repositoryType)
        {
            Assembly repositoriesAssembly = repositoryType.Assembly;
            IEnumerable<Type> repositoryInterfaces = repositoriesAssembly
                .GetTypes()
                .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Repository"))
                .ToArray();

            Type[] assemblyTypes = repositoriesAssembly.GetTypes();

            foreach (Type serviceType in repositoryInterfaces)
            {
                Type implementationType = FindImplementationType(assemblyTypes, serviceType);
                serviceCollection.AddScoped(serviceType, implementationType);
            }

            return serviceCollection;
        }


        public static IServiceCollection RegisterUserServices(this IServiceCollection serviceCollection, Type serviceType)
        {
            Assembly servicesAssembly = serviceType.Assembly;
            IEnumerable<Type> serviceInterfaces = servicesAssembly
                .GetTypes()
                .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Service"))
                .ToArray();

            Type[] assemblyTypes = servicesAssembly.GetTypes();

            foreach (Type currentServiceType in serviceInterfaces)
            {
                Type implementationType = FindImplementationType(assemblyTypes, currentServiceType);
                serviceCollection.AddScoped(currentServiceType, implementationType);
            }

            return serviceCollection;
        }

        private static Type FindImplementationType(IEnumerable<Type> assemblyTypes, Type contractType)
        {
            string expectedImplementationName = contractType.Name[1..];

            Type? implementationType = assemblyTypes
                .SingleOrDefault(t => t.IsClass
                    && !t.IsAbstract
                    && t.Name.Equals(expectedImplementationName, StringComparison.Ordinal));

            if (implementationType is not null)
            {
                return implementationType;
            }

            Type[] matchingTypes = assemblyTypes
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && contractType.IsAssignableFrom(t))
                .ToArray();

            return matchingTypes.Length switch
            {
                1 => matchingTypes[0],
                0 => throw new InvalidOperationException($"No implementation found for {contractType.FullName}."),
                _ => throw new InvalidOperationException($"Multiple implementations found for {contractType.FullName}: {string.Join(", ", matchingTypes.Select(t => t.FullName))}")
            };
        }
    }
}
