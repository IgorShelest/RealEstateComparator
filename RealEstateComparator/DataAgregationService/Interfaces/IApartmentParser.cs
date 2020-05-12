using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;

namespace DataAggregationService.Interfaces
{
    public interface IApartmentParser
    {
        Task<IEnumerable<ApartComplex>> GetApartmentData();
    }
}
