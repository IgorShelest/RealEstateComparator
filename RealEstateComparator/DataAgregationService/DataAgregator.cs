﻿using System.Collections.Generic;
using System.Linq;
using DataAgregationService.Db;
using DataAgregationService.Models;
using DataAgregationService.Parsers;

namespace DataAgregationService
{
    class DataAgregator
    {
        // Db data
        private ApplicationContext _dbContext;
        private IEnumerable<ApartComplex> _apartComplexes;

        // Internal data
        private List<IApartmentParser> _parsers;

        public DataAgregator()
        {
            _dbContext = new ApplicationContext();

            _parsers = new List<IApartmentParser>() { new LunUaApartmentParser() };
        }

        public void Run()
        {
            GetData();
            ValidateData();
            UpdateDb();
        }

        private void GetData()
        {
            foreach (var parser in _parsers)
                _apartComplexes = _apartComplexes.Concat(parser.ParseApartmentData());
        }

        private void ValidateData()
        {
            _apartComplexes = _apartComplexes.Where(complex => complex.Apartments != null);
        }

        private void UpdateDb()
        {
            _dbContext.ApartComplexes.AddRange(_apartComplexes);
            _dbContext.SaveChanges();
        }
    }
}
