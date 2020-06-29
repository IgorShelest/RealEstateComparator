using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using ApplicationContextRepositories;
using DataAggregationService.Interfaces;

namespace DataAggregationService
{
    public class DataAggregator
    {
        private readonly IApartComplexRepository _apartComplexRepository;
        private IEnumerable<IAggregator> _aggregators;
        private IEnumerable<ApartComplex> _apartComplexes;

        public DataAggregator(IApartComplexRepository apartComplexRepository, IEnumerable<IAggregator> aggregators)
        {
            _apartComplexRepository = apartComplexRepository;
            _aggregators = aggregators;
        }

        public async Task Run()
        {
            await GetData();
            ValidateData();
            PrintData();
            await UpdateDb();
        }

        private async Task GetData()
        {
            var getDataTasks = _aggregators.Select(aggregator => aggregator.AggregateData());
            var getDataResults = await Task.WhenAll(getDataTasks);
            
            var apartComplexes = new List<ApartComplex>();
            getDataResults.ToList().ForEach(result => apartComplexes.AddRange(result));
            _apartComplexes = apartComplexes;
        }

        private void ValidateData()
        {
            _apartComplexes = _apartComplexes.Where(complex => complex.Apartments != null);
        }

        private async Task UpdateDb()
        {
            await _apartComplexRepository.UpdateDb(_apartComplexes);
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
