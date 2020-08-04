using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.DomRia.Services;
using DataAggregationService.Interfaces;

namespace DataAggregationService.Aggregators.DomRia
{
    public class DomRiaAggregator: IAggregator
    {
        private readonly ApartComplexHandler _apartComplexHandler;
        private readonly ApartmentHandler _apartmentHandler;

        public DomRiaAggregator(ApartComplexHandler apartComplexHandler, ApartmentHandler apartmentHandler)
        {
            _apartComplexHandler = apartComplexHandler;
            _apartmentHandler = apartmentHandler;
        }

        public virtual async Task<IEnumerable<ApartComplex>> AggregateData()
        {
            var apartComplexes = await _apartComplexHandler.GetApartComplexes();
            await _apartmentHandler.SetApartments(apartComplexes);

            return apartComplexes;
        }
    }
}