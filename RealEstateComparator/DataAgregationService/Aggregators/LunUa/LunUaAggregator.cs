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
        private readonly ApartmentHandler _apartmentHandler;

        public LunUaAggregator()
        {
            _cityHandler = new CityHandler();
            _apartComplexHandler = new ApartComplexHandler();
            _apartmentHandler = new ApartmentHandler();
        }

        public async Task<IEnumerable<ApartComplex>> AggregateData()
        {
            var citiesData = await _cityHandler.GetCityData();
            var apartComplexes = await _apartComplexHandler.GetApartComplexes(citiesData);
            await _apartmentHandler.SetApartments(apartComplexes);
            
            return apartComplexes;
        }
    }
}
