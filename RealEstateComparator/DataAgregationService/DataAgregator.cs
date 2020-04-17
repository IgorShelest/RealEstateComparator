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
            _apartComplexes = new List<ApartComplex>();
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
            var getDataTasks = await StartGetDataTasks();
            //AddGetDataResults(getDataTasks);
        }

        private async Task<List<IEnumerable<ApartComplex>[]>> StartGetDataTasks()
        {
            var taskResultList = new List<IEnumerable<ApartComplex>[]>();

            foreach (var parser in _parsers)
                taskResultList.Add(await parser.GetApartmentData());

            return taskResultList;
        }

        private void AddGetDataResults(IEnumerable<Task<IEnumerable<ApartComplex>>> getDataTasks)
        {
            foreach (var task in getDataTasks)
                _apartComplexes = _apartComplexes.Concat(task.Result.ToList());
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
