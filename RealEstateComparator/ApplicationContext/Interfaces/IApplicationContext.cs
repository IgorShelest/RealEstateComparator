using ApplicationContext.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationContext
{
    public interface IApplicationContext
    {
        DbSet<ApartComplex> ApartComplexes { get; set; }
        DbSet<Apartment> Apartments { get; set; }

        void Save();
    }
}