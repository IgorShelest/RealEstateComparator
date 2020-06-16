using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Interfaces;
using DataAggregationService.Parsers.DomRia.Services;

namespace DataAggregationService.Parsers.DomRia
{
    public class DomRiaAggregator: IAggregator
    {
        private readonly ApartComplexHandler _apartComplexHandler;
        private readonly ApartmentHandler _apartmentHandler;

        public DomRiaAggregator()
        {
            _apartComplexHandler = new ApartComplexHandler();
            _apartmentHandler = new ApartmentHandler();
        }

        public async Task<IEnumerable<ApartComplex>> AggregateData()
        {
            var apartComplexes = await _apartComplexHandler.GetApartComplexes();
            await _apartmentHandler.SetApartments(apartComplexes);

            return apartComplexes;
        }
    }
}