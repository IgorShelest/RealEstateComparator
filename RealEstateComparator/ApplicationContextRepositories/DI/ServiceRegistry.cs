using ApplicationContexts;
using ApplicationContexts.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationContextRepositories.DI
{
    public static class ServiceRegistry
    {
        public static IServiceCollection AddApplicationContextRepositories(this IServiceCollection services)
        {
            services.AddScoped<IApplicationContext, MySQLContext>();
            return services;
        }
    }
}