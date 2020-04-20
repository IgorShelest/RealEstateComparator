using System.Collections.Generic;
using System.IO;
using DataAgregationService.Models;
using DataAgregationService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DbService
{
    public class DbService : DbContext
    {
        private DbSet<ApartComplex> _apartComplexes { get; set; }
        private DbSet<Apartment> _apartments { get; set; }
        
        public DbService()
        {
            Database.Migrate();
        }

        public void UpdateDbMy(IEnumerable<ApartComplex> apartComplexes)
        {
            _apartComplexes.AddRange(apartComplexes);
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