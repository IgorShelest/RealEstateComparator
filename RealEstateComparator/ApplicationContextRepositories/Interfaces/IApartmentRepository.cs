using System.Collections.Generic;
using ApplicationContexts.Models;
using ApplicationContextRepositories.Dto;

namespace ApplicationContextRepositories
{
    public interface IApartmentRepository
    {
        IEnumerable<Apartment> GetApartments(ApartmentSpecsDto apartmentSpecs);
    }
}