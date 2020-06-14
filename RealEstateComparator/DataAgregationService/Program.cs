using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContexts;
using DataAggregationService.Interfaces;
using DataAggregationService.Parsers.DomRia;
using DataAggregationService.Parsers.LunUa;
using DataAggregationService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DataAggregationService
{
    static class Program
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

            serviceCollection.AddSingleton<IConfigHandler>(service => new ConfigHandler("appsettings.json"));
            serviceCollection.AddScoped<DataAggregator>();
            serviceCollection.AddScoped<IApplicationContext>(service => new SQLServerContext(service.GetRequiredService<IConfigHandler>().GetConnectionString("DefaultConnection")));
            serviceCollection.AddScoped<IApartComplexRepository>(service => new ApartComplexRepository(service.GetRequiredService<IApplicationContext>()));
            serviceCollection.AddScoped<IAggregator, LunUaAggregator>();
            // serviceCollection.AddScoped<IAggregator, DomRiaAggregator>();
            
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
