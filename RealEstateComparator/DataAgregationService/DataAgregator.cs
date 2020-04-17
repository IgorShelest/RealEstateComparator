using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private IEnumerable<IApartmentParser> _parsers;

        public DataAgregator()
        {
            _dbContext = new ApplicationContext();
            _parsers = new List<IApartmentParser>();
        }

        public async Task Run()
        {
            InitializeParsers();
            await GetData();
            ValidateData();
            PrintData();
            UpdateDb();
        }

        private void InitializeParsers()
        {
            _parsers = _parsers.Append(ParserFactory.CreateParser<LunUaApartmentParser>());
        }

        private async Task GetData()
        {
            var getDataTasks = _parsers.Select(parser => parser.GetApartmentData());
            var getDataResults = await Task.WhenAll(getDataTasks);
            
            var apartComplexes = new List<ApartComplex>();
            getDataResults.ToList().ForEach(result => apartComplexes.AddRange(result));
            _apartComplexes = apartComplexes;
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
