using System.Collections.Generic;
using ApplicationContexts.Models;
using ApplicationContextRepositories.Dto;

namespace RealEstateComparatorService.Services
{
    public interface IRealEstateService
    {
        IEnumerable<Apartment> GetBetterApartments(ApartmentSpecsDto apartmentSpecs);
    }
}