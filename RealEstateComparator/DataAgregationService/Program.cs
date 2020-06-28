using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContexts;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Interfaces;
using DataAggregationService.Aggregators.DomRia;
using DataAggregationService.Services;
using DataAgregationService.Aggregators.LunUa;
using DataAgregationService.Agregators.LunUa.Services;
using HtmlAgilityPack;
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
            serviceCollection.AddSingleton<HtmlParser>();
            serviceCollection.AddSingleton<HtmlWeb>();
            serviceCollection.AddScoped<DataAggregator>();
            serviceCollection.AddScoped<IApplicationContext>(service => new SQLServerContext(service.GetRequiredService<IConfigHandler>().GetConnectionString("DefaultConnection")));
            serviceCollection.AddScoped<IApartComplexRepository>(service => new ApartComplexRepository(service.GetRequiredService<IApplicationContext>()));
            serviceCollection.AddScoped<IAggregator, LunUaAggregator>();
            // serviceCollection.AddScoped<IAggregator, DomRiaAggregator>();
            serviceCollection.AddScoped<PageHandler>();
            serviceCollection.AddScoped<CityHandler>();
            serviceCollection.AddScoped<ApartComplexHandler>();
            serviceCollection.AddScoped<ApartmentHandler>();
            
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
