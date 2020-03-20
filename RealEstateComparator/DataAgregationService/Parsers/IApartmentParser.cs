using DataAgregationService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.Parsers
{
    interface IApartmentParser
    {
        ICollection<ApartComplex> ParseApartmentData();
    }
}
