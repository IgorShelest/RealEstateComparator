using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using ApplicationContextRepositories.Dto;

namespace RealEstateComparatorService.Services
{
    public interface IRealEstateService
    {
        IEnumerable<Apartment> GetBetterApartments(ApartmentSpecsDto apartmentSpecs);

        Task<ApartComplex> GetApartComplex(int apartComplexId);
    }
}