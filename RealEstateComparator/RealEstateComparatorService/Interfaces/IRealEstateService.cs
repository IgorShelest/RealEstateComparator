using System.Collections.Generic;
using ApplicationContext.Models;
using ApplicationContextRepositories.Dto;

namespace RealEstateComparatorService.Services
{
    public interface IRealEstateService
    {
        IEnumerable<Apartment> GetBetterApartments(ApartmentSpecsDto apartmentSpecs);
    }
}