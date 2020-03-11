using DataAgregationService.DataSources;
using DataAgregationService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.Parsers
{
    interface IParser
    {
        ICollection<ApartComplex> ParseSpecificData();
    }
}
