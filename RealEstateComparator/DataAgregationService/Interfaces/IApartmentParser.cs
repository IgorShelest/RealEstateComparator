using System.Collections.Generic;
using System.Threading.Tasks;
using DataAgregationService.Models;

namespace DataAgregationService.Parsers
{
    interface IApartmentParser
    {
        Task<IEnumerable<ApartComplex>> GetApartmentData();
    }
}
