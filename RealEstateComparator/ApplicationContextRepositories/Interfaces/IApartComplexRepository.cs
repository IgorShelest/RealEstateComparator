using System.Collections.Generic;
using ApplicationContext.Models;

namespace ApplicationContextRepositories
{
    public interface IApartComplexRepository
    {
        void UpdateDb(IEnumerable<ApartComplex> apartComplexes);
    }
}