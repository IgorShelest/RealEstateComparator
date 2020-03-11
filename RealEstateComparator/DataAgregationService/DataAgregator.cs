using DataAgregationService.DataSources;
using DataAgregationService.Db;
using DataAgregationService.Models;
using DataAgregationService.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataAgregationService
{
    class DataAgregator
    {
        // Db data
        private ApplicationContext _dbContext;

        private ICollection<ApartComplex> _apartComplexes;

        // Internal data
        private List<IDataSource> _dataSources;

        public DataAgregator( )
        {
            _dbContext = new ApplicationContext();

            _dataSources = new List<IDataSource>() {new LunUa()};
        }

        public void Run()
        {
            GetData();
            ValidateData();
            UpdateDb();
        }

        private void GetData()
        {
            var apartComplexParser = new LunUaParser();

            _apartComplexes = apartComplexParser.ParseSpecificData();
        }

        private void ValidateData()
        {
            var complexesWithoutApartments = _apartComplexes.Where(complex => complex.Apartments == null).ToList();

            foreach (var complex in complexesWithoutApartments)
            {                
                _apartComplexes.Remove(complex);
            }
        }

        private void UpdateDb()
        {
            _dbContext.ApartComplexes.AddRange(_apartComplexes);
            _dbContext.SaveChanges();
        }
    }
}
