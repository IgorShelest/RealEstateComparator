using ApplicationContexts.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationContexts
{
    public class SQLServerContext : DbContext, IApplicationContext
    {
        public DbSet<ApartComplex> ApartComplexes { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        private readonly string _connectionString;

        public SQLServerContext()
        {
            if (_connectionString == null)
                _connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'New Database'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
        }

        public SQLServerContext(string connectionString)
        {
            _connectionString = connectionString;

            Database.Migrate();
            //Database.EnsureCreated();
        }

        public void Save()
        {
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
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
    }
}