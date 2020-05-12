using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationContextRepositories;
using ApplicationContextRepositories.Dto;
using ApplicationContexts.Models;

namespace RealEstateComparatorService.Services
{
    public class RealEstateService : IRealEstateService
    {
        private readonly IApartmentRepository _apartmentRepository;

        public RealEstateService(IApartmentRepository apartmentRepository)
        {
            _apartmentRepository = apartmentRepository;
        }

        public IEnumerable<Apartment> GetBetterApartments(ApartmentSpecsDto apartmentSpecs)
        {
            var apartmentsByPhysicalSpecs = SelectApartmentsByPhysicalsSpecs(apartmentSpecs);
            var betterApartments = SelectApartmentsByPriceSpecs(apartmentsByPhysicalSpecs, apartmentSpecs);

            return betterApartments.ToList();
        }

        private IEnumerable<Apartment> SelectApartmentsByPhysicalsSpecs(ApartmentSpecsDto apartmentSpecs)
        {
            return _apartmentRepository.GetApartments(apartmentSpecs);
        }

        private int CalculateSquareMeterPrice(Apartment apartment, int dwellingSpace)
        {
            var spaceDiff = apartment.DwellingSpaceMax - apartment.DwellingSpaceMin;
            var priceDiff = apartment.SquareMeterPriceMax - apartment.SquareMeterPriceMin;

            var priceDiffPerMeter = priceDiff / spaceDiff;

            var addedSpace = dwellingSpace - apartment.DwellingSpaceMin;
            var addedPrice = addedSpace * priceDiffPerMeter;

            var squareMeterPrice = apartment.SquareMeterPriceMax - addedPrice;

            return squareMeterPrice;
        }

        private int CalculateApartmentPrice(Apartment apartment, int dwellingSpace)
        {
            var squareMeterPrice = CalculateSquareMeterPrice(apartment, dwellingSpace);
            var apartmentPrice = squareMeterPrice * dwellingSpace;

            return apartmentPrice;
        }

        private IEnumerable<Apartment> SelectApartmentsByPriceSpecs(IEnumerable<Apartment> comparableApartments, ApartmentSpecsDto apartmentSpecs)
        {
            try
            {
                int generalRenovationPrice = apartmentSpecs.DwellingSpace * apartmentSpecs.RenovationPricePerMeter;

                var apartments = comparableApartments
                     .Where(delegate (Apartment apartment)
                     {
                         var apartPriceWithoutRenovation = CalculateApartmentPrice(apartment, apartmentSpecs.DwellingSpace);
                         var apartPriceWithRenovation = apartPriceWithoutRenovation + generalRenovationPrice;

                         var isItProfitable = apartPriceWithRenovation < apartmentSpecs.OverallPrice;
                         return isItProfitable;
                     });

                return apartments;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }
    }
}
