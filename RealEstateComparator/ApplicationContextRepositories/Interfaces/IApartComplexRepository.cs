using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;

namespace ApplicationContextRepositories
{
    public interface IApartComplexRepository
    {
        Task UpdateDb(IEnumerable<ApartComplex> apartComplexes);
        
        Task<ApartComplex> GetApartComplex(int ComplexId);
    }
}