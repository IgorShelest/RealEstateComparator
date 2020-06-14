using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;

namespace DataAggregationService.Interfaces
{
    public interface IAggregator
    {
        Task<IEnumerable<ApartComplex>> GetApartmentData();
    }
}
