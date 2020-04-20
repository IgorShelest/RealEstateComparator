using System.Collections.Generic;
using System.Threading.Tasks;
using DataAgregationService.Models;

namespace DataAgregationService.Interfaces
{
    interface IApartmentParser
    {
        Task<IEnumerable<ApartComplex>> GetApartmentData();
    }
}
