using System.Collections.Generic;
using DataAgregationService.Models;

namespace DataAgregationService.Interfaces
{
    public interface IDbService
    {
        void UpdateDb(IEnumerable<ApartComplex> apartComplexes);
    }
}