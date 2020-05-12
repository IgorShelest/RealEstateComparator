using System.Collections.Generic;
using ApplicationContexts.Models;

namespace ApplicationContextRepositories
{
    public interface IApartComplexRepository
    {
        void UpdateDb(IEnumerable<ApartComplex> apartComplexes);
    }
}