using System.Collections.Generic;
using DataAgregationService.Models;

namespace DataAgregationService.Parsers
{
    interface IApartmentParser
    {
        IEnumerable<ApartComplex> ParseApartmentData();
    }
}
