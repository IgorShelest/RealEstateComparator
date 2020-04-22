using System.Collections.Generic;
using ApplicationContext;
using ApplicationContext.Models;

namespace ApplicationContextRepositories
{
    public class ApartComplexRepository : IApartComplexRepository
    {
        private readonly IApplicationContext _applicationContext;

        public ApartComplexRepository()
        {
            _applicationContext = new MySQLContext();
        }

        public void UpdateDb(IEnumerable<ApartComplex> apartComplexes)
        {
            _applicationContext.ApartComplexes.AddRange(apartComplexes);
            _applicationContext.Save();
        }
    }
}