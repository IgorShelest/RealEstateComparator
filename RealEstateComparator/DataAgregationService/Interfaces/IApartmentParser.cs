using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContext.Models;

namespace DataAggregationService.Interfaces
{
    public interface IApartmentParser
    {
        Task<IEnumerable<ApartComplex>> GetApartmentData();
    }
}
