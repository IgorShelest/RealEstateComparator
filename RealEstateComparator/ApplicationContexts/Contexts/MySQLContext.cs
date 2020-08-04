using ApplicationContexts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApplicationContexts
{
    public class MySQLContext: DbContext, IApplicationContext
    {
        public DbSet<ApartComplex> ApartComplexes { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        private readonly string  _connectionString;

        /*
        * Called automatically by application
        */
        public MySQLContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.Migrate();
        }
        
        /*
        * Called manually for Migration Creation
        */
        public MySQLContext()
        {
            _connectionString = ReadConnectionString();
            Database.Migrate();
        }

        public void Save()
        {
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connectionString);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Apartment>(entity =>
            {
                entity.HasOne(a => a.Complex);
                entity.HasKey(a => a.Id);
                entity.Property(a => a.HasMultipleFloors).IsRequired();
                entity.Property(a => a.DwellingSpaceMin).IsRequired();
                entity.Property(a => a.DwellingSpaceMax).IsRequired();
                entity.Property(a => a.SquareMeterPriceMin).IsRequired();
                entity.Property(a => a.SquareMeterPriceMax).IsRequired();
            });

            modelBuilder.Entity<ApartComplex>(entity =>
            {
                entity.HasMany(c => c.Apartments);
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Source).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Name).HasMaxLength(50).IsRequired();
                entity.Property(c => c.CityName).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Url).HasMaxLength(200).IsRequired();
            });
            
            base.OnModelCreating(modelBuilder);
        }

        private string ReadConnectionString()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json");
            var configRoot = configBuilder.Build();
            return configRoot.GetConnectionString("DefaultMySQLConnection");   
        }
    }
}