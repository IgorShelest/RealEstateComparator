using System;
using System.Collections.Generic;
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
            PrintData();
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

        private void PrintData()
        {
            foreach (var apartComplex in _apartComplexes)
            {
                Console.WriteLine(apartComplex.Name + " " + apartComplex.CityName + " " + apartComplex.Url);

                if (apartComplex.Apartments != null)
                {
                    foreach (var apartment in apartComplex.Apartments)
                        Console.WriteLine(apartment.NumberOfRooms + " " + apartment.DwellingSpaceMin + " " + apartment.DwellingSpaceMax + " " + apartment.SquareMeterPriceMin + " " + apartment.SquareMeterPriceMax);
                }
            }
        }
    }
}
