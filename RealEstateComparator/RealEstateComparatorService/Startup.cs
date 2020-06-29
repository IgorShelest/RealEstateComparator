using System;
using ApplicationContextRepositories;
using ApplicationContexts;
using AutoMapper;
using DataAggregationService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealEstateComparatorService.Services;

namespace RealEstateComparatorService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IRealEstateService, RealEstateService>();
            services.AddScoped<IApplicationContext>(service => new SQLServerContext(Configuration.GetValue<string>("ConnectionStrings:DefaultConnection")));
            services.AddScoped<IApartmentRepository, ApartmentRepository>();
            services.AddScoped<IApartComplexRepository, ApartComplexRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());  

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
