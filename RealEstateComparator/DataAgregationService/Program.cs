using System;
using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContextRepositories.DI;
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
            serviceCollection.AddScoped<IApartComplexRepository, ApartComplexRepository>();
            serviceCollection.AddScoped<IApartmentParser, LunUaApartmentParser>();
            serviceCollection.AddApplicationContextRepositories();
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
