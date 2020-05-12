using ApplicationContexts.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationContexts
{
    public interface IApplicationContext
    {
        DbSet<ApartComplex> ApartComplexes { get; set; }
        DbSet<Apartment> Apartments { get; set; }

        void Save();
    }
}