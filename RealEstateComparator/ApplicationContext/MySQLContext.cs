using System.IO;
using ApplicationContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApplicationContext
{
    public class MySQLContext: DbContext, IApplicationContext
    {
        public DbSet<ApartComplex> ApartComplexes { get; set; }
        public DbSet<Apartment> Apartments { get; set; }

        public MySQLContext()
        {
            // Database.Migrate();
            Database.EnsureCreated();
        }

        public void Save()
        {
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile("appsettings.json");
        
            var config = configBuilder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");
        
            optionsBuilder.UseMySql(connectionString);
        }
    }
}