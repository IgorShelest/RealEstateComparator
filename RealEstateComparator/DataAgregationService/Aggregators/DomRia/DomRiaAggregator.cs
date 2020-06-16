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

        public DomRiaAggregator()
        {
            _apartComplexHandler = new ApartComplexHandler();
        }

        public async Task<IEnumerable<ApartComplex>> GetApartmentData()
        {
            return await _apartComplexHandler.GetApartComplexes();
        }
    }
}