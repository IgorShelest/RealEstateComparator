using DataAgregationService.DataSources;
using DataAgregationService.Db;
using DataAgregationService.Models;
using DataAgregationService.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace DataAgregationService
{
    class DataAgregator
    {
        // Db data
        private ApplicationContext _dbContext;

        private List<ApartComplex> _apartComplexes;

        // Internal data
        private List<IDataSource> _dataSources;

        public DataAgregator( )
        {
            _dbContext = new ApplicationContext();

            _apartComplexes = new List<ApartComplex>();

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
            var apartComplexes = apartComplexParser.ParseSpecificData();
            _apartComplexes.AddRange(apartComplexes);
        }

        private void ValidateData()
        {
            foreach (var apartComplex in _apartComplexes)
            {
                if (apartComplex.Apartments == null)
                    _apartComplexes.Remove(apartComplex);
            }
        }

        private void UpdateDb()
        {
        }
    }
}
