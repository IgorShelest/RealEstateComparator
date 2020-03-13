using DataAgregationService.Db;
using DataAgregationService.Models;
using RealEstateComparatorService.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateComparatorService.Services
{
    public class RealEstateService
    {
        private ApplicationContext _dbContext = new ApplicationContext();

        private IQueryable<Apartment> SelectApartmentsByPhisicalSpecs(ApartmentSpecifications apartmentSpecs)
        {
            try
            {
                var apartments = _dbContext.Apartments
                    .Where(apartment => apartmentSpecs.City == apartment.Complex.CityName)
                    .Where(apartment => apartmentSpecs.NumberOfRooms == apartment.NumberOfRooms)
                    .Where(apartment => (apartmentSpecs.DwellingSpace >= apartment.DwellingSpaceMin && apartmentSpecs.DwellingSpace <= apartment.DwellingSpaceMax));

                return apartments;
            }
            catch (Exception ex)
            { 
                // Log
            }

            return null;
        }

        private int calculateSquareMeterPrice(Apartment apartment, int dwellingSpace)
        {
            var spaceDiff = apartment.DwellingSpaceMax - apartment.DwellingSpaceMin;
            var priceDiff = apartment.SquareMeterPriceMax - apartment.SquareMeterPriceMin;

            var priceDiffPerMeter = priceDiff / spaceDiff;

            var addedSpace = dwellingSpace - apartment.DwellingSpaceMin;
            var addedPrice = addedSpace * priceDiffPerMeter;

            var squareMeterPrice = apartment.SquareMeterPriceMax - addedPrice;

            return squareMeterPrice;
        }

        private int calculateApartmentPrice(Apartment apartment, int dwellingSpace)
        {
            var squareMeterPrice = calculateSquareMeterPrice(apartment, dwellingSpace);
            var apartmentPrice = squareMeterPrice * dwellingSpace;

            return apartmentPrice;
        }

        private IQueryable<Apartment> SelectApartmentsByPrice(IEnumerable<Apartment> comparableApartments, ApartmentSpecifications apartmentSpecs)
        {
            try
            {
                int generalRenovationPrice = apartmentSpecs.DwellingSpace * apartmentSpecs.RenovationPricePerMeter;

                var apartments = comparableApartments
                     .Where(delegate (Apartment apartment)
                     {
                         var apartPriceWithoutRenovation = calculateApartmentPrice(apartment, apartmentSpecs.DwellingSpace);
                         var apartPriceWithRenovation = apartPriceWithoutRenovation + generalRenovationPrice;

                         var isItProfitable = apartPriceWithRenovation < apartmentSpecs.OverallPrice;
                         return isItProfitable;
                     })
                     .AsQueryable();

                return apartments;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public IEnumerable<Apartment> GetBetterApartments(ApartmentSpecifications apartmentSpecs)
        {
            var apartmentsByPhisicalSpecs = SelectApartmentsByPhisicalSpecs(apartmentSpecs);
            var betterApartments = SelectApartmentsByPrice(apartmentsByPhisicalSpecs, apartmentSpecs);

            return betterApartments.ToList();
        }
    }
}
