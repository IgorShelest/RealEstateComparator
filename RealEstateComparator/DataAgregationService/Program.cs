using System;
using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContexts;
using DataAggregationService.Interfaces;
using DataAggregationService.Parsers.LunUa;
using Microsoft.Extensions.DependencyInjection;

namespace DataAggregationService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var scope = CreateScope();
            var dataAggregator = scope.ServiceProvider.GetRequiredService<DataAggregator>();
            await dataAggregator.Run();
        }

        private static ServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddScoped<DataAggregator>();
            
            serviceCollection.AddScoped<IApplicationContext, MySQLContext>();
            serviceCollection.AddScoped<IApartComplexRepository>(service => new ApartComplexRepository(service.GetRequiredService<IApplicationContext>()));
            
            serviceCollection.AddScoped<IApartmentParser, LunUaApartmentParser>();
            
            return serviceCollection;
        }

        private static IServiceScope CreateScope()
        {
            var serviceCollection = ConfigureServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.CreateScope();
        }
    }
}
