using System.Collections.Generic;
using System.Linq;
using ApplicationContext;
using ApplicationContext.Models;
using ApplicationContextRepositories.Dto;

namespace ApplicationContextRepositories
{
    public class ApartmentRepository: IApartmentRepository
    {
        private readonly IApplicationContext _applicationContext;

        public ApartmentRepository(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public IEnumerable<Apartment> GetApartments(ApartmentSpecsDto apartmentSpecs)
        {
            var apartments = _applicationContext.Apartments
                .Where(apartment => apartmentSpecs.City == apartment.Complex.CityName)
                .Where(apartment => apartmentSpecs.NumberOfRooms == apartment.NumberOfRooms)
                .Where(apartment => apartmentSpecs.HasMultipleFloors == apartment.HasMultipleFloors)
                .Where(apartment => (apartmentSpecs.DwellingSpace >= apartment.DwellingSpaceMin && apartmentSpecs.DwellingSpace <= apartment.DwellingSpaceMax));

            return apartments;
        }
    }
}