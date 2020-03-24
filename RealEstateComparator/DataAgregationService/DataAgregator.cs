using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAgregationService.Db;
using DataAgregationService.Enums;
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
            _apartComplexes = new List<ApartComplex>();
            _parsers = new List<IApartmentParser>();
        }

        public void Run()
        {
            InitializeParsers();
            GetData();
            ValidateData();
            PrintData();
            UpdateDb();
        }

        private void InitializeParsers()
        {
            _parsers.Add(ParserFactory.CreateParser(AvailableParsers.LunUa));
        }

        private async void GetData()
        {
            var taskResultList = _parsers.Select(parser => Task.Run(parser.ParseApartmentData));

            Task.WaitAll(taskResultList.ToArray());

            taskResultList.ToList().ForEach(task => _apartComplexes = _apartComplexes.Concat(task.Result));
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
