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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Apartment>().HasOne(a => a.Complex);
            modelBuilder.Entity<Apartment>().HasKey(a => a.Id);
            modelBuilder.Entity<Apartment>().Property(a => a.NumberOfRooms).IsRequired();
            modelBuilder.Entity<Apartment>().Property(a => a.DwellingSpaceMin).IsRequired();
            modelBuilder.Entity<Apartment>().Property(a => a.DwellingSpaceMax).IsRequired();
            modelBuilder.Entity<Apartment>().Property(a => a.SquareMeterPriceMin).IsRequired();
            modelBuilder.Entity<Apartment>().Property(a => a.SquareMeterPriceMax).IsRequired();

            modelBuilder.Entity<ApartComplex>().HasMany(c => c.Apartments);
            modelBuilder.Entity<ApartComplex>().HasKey(c => c.Id);
            modelBuilder.Entity<ApartComplex>().Property(c => c.Name).IsRequired();
            modelBuilder.Entity<ApartComplex>().Property(c => c.CityName).IsRequired();
            modelBuilder.Entity<ApartComplex>().Property(c => c.Url).IsRequired();
            
            base.OnModelCreating(modelBuilder);
        }
    }
}