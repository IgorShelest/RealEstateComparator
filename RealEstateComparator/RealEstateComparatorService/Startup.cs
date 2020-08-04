using System;
using System.IO;
using System.Reflection;
using ApplicationContextRepositories;
using ApplicationContexts;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
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
            services.AddScoped<IApplicationContext>(service => new SQLServerContext(Configuration.GetValue<string>("ConnectionStrings:DefaultSQLServerConnection")));
            services.AddScoped<IApartmentRepository, ApartmentRepository>();
            services.AddScoped<IApartComplexRepository, ApartComplexRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());  
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Real Estate Comparator API", Version = "v1" });
                
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swagger.IncludeXmlComments(xmlPath);
            });
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
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Real Estate Comparator API V1");
            });
        }
    }
}
