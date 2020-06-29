using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.LunUa.Services;
using DataAggregationService.Interfaces;

namespace DataAggregationService.Aggregators.LunUa
{
    public class LunUaAggregator : IAggregator
    {
        private readonly CityHandler _cityHandler;
        private readonly ApartComplexHandler _apartComplexHandler;
        private readonly ApartmentHandler _apartmentHandler;

        public LunUaAggregator(CityHandler cityHandler, ApartComplexHandler apartComplexHandler, ApartmentHandler apartmentHandler)
        {
            _cityHandler = cityHandler;
            _apartComplexHandler = apartComplexHandler;
            _apartmentHandler = apartmentHandler;
        }

        public virtual async Task<IEnumerable<ApartComplex>> AggregateData()
        {
            var citiesData = await _cityHandler.GetCityData();
            var apartComplexes = await _apartComplexHandler.GetApartComplexes(citiesData);
            await _apartmentHandler.SetApartments(apartComplexes);
            
            return apartComplexes;
        }
    }
}
