using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Interfaces;
using DataAgregationService.Parsers.LunUa;

namespace DataAggregationService.Parsers.LunUa
{
    class LunUaAggregator : IAggregator
    {
        private readonly CityHandler _cityHandler;
        private readonly ApartComplexHandler _apartComplexHandler;

        public LunUaAggregator()
        {
            _cityHandler = new CityHandler();
            _apartComplexHandler = new ApartComplexHandler();
        }

        public async Task<IEnumerable<ApartComplex>> GetApartmentData()
        {
            var citiesData = await _cityHandler.GetCityData();
            var apartComplexes = await _apartComplexHandler.GetApartComplexes(citiesData);
            return apartComplexes;
        }
    }
}
