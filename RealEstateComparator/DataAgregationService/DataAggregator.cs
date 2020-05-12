using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using ApplicationContextRepositories;
using DataAggregationService.Interfaces;
using DataAggregationService.Parsers.LunUa;
using Microsoft.Extensions.DependencyInjection;

namespace DataAggregationService
{
    public class DataAggregator
    {
        private readonly IApartComplexRepository _apartComplexRepository;
        private IEnumerable<IApartmentParser> _parsers;
        private IEnumerable<ApartComplex> _apartComplexes;

        public DataAggregator(IApartComplexRepository apartComplexRepository, IEnumerable<IApartmentParser> parsers)
        {
            _apartComplexRepository = apartComplexRepository;
            _parsers = parsers;
        }

        public async Task Run()
        {
            await GetData();
            ValidateData();
            PrintData();
            UpdateDb();
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
            _apartComplexRepository.UpdateDb(_apartComplexes);
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
