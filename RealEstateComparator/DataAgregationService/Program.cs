using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContexts;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Interfaces;
using DataAggregationService.Aggregators.DomRia;
using DataAggregationService.Services;
using DataAggregationService.Aggregators;
using DataAggregationService.Aggregators.LunUa.Services;
using DataAggregationService.Aggregators.DomRia.Services;
using DataAggregationService.Aggregators.LunUa;
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
            serviceCollection.AddScoped<IApartComplexRepository, ApartComplexRepository>();
            
            serviceCollection.AddScoped<IAggregator, LunUaAggregator>();
            serviceCollection.AddScoped<DataAggregationService.Aggregators.LunUa.Services.PageHandler>();
            serviceCollection.AddScoped<CityHandler>();
            serviceCollection.AddScoped<DataAggregationService.Aggregators.LunUa.Services.ApartComplexHandler>();
            serviceCollection.AddScoped<DataAggregationService.Aggregators.LunUa.Services.ApartmentHandler>();
            
            // serviceCollection.AddScoped<IAggregator, DomRiaAggregator>();
            // serviceCollection.AddScoped<DataAggregationService.Aggregators.DomRia.Services.PageHandler>();
            // serviceCollection.AddScoped<CityHandler>();
            // serviceCollection.AddScoped<DataAggregationService.Aggregators.DomRia.Services.ApartComplexHandler>();
            // serviceCollection.AddScoped<DataAggregationService.Aggregators.DomRia.Services.ApartmentHandler>();
            
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
