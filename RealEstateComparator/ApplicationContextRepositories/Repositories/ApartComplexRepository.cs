using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts;
using ApplicationContexts.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationContextRepositories
{
    public class ApartComplexRepository : IApartComplexRepository
    {
        private readonly IApplicationContext _applicationContext;

        public ApartComplexRepository(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public async Task UpdateDb(IEnumerable<ApartComplex> apartComplexes)
        {
            await _applicationContext.ApartComplexes.AddRangeAsync(apartComplexes);
            _applicationContext.Save();
        }

        public virtual async Task<ApartComplex> GetApartComplex(int complexId)
        {
            return await _applicationContext.ApartComplexes.FirstOrDefaultAsync(complex => complex.Id == complexId);
        }
    }
}