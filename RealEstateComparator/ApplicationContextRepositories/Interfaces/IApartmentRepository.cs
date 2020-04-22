using System.Collections.Generic;
using ApplicationContext.Models;
using ApplicationContextRepositories.Dto;

namespace ApplicationContextRepositories
{
    public interface IApartmentRepository
    {
        IEnumerable<Apartment> GetApartments(ApartmentSpecsDto apartmentSpecs);
    }
}