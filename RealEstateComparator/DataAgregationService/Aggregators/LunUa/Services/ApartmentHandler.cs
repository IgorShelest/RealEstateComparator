using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using HtmlAgilityPack;

namespace DataAgregationService.Aggregators.LunUa
{
    public class ApartmentHandler
    {
        private readonly PageHandler _pageHandler;

        public ApartmentHandler(PageHandler pageHandler)
        {
            _pageHandler = pageHandler;
        }

        public async Task SetApartments(IEnumerable<ApartComplex> apartComplexes)
        {
            foreach (var complex in apartComplexes)
                complex.Apartments = await GetApartmentsPerApartComplex(complex.Url);
        }

        private async Task<IEnumerable<Apartment>> GetApartmentsPerApartComplex(string url)
        {
            try
            {
                var htmlNodes = await _pageHandler.LoadApartmentsHtml(url);
                var apartments = CreateApartmentsPerApartComplex(htmlNodes);
                return apartments;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private IEnumerable<Apartment> CreateApartmentsPerApartComplex(HtmlNodeCollection htmlNodes)
        {
            var apartments = htmlNodes?.Select(CreateApartment).ToList();
            return apartments;
        }
        
        private Apartment CreateApartment(HtmlNode node)
        {
            var numOfRooms = _pageHandler.ParseHtmlNumOfRooms(node);
            var hasMultipleFloors = _pageHandler.HasMultipleFloors(node);
            var roomSpace = _pageHandler.ParseHtmlRoomSpace(node);
            var price = _pageHandler.ParseHtmlApartPrice(node);

            return new Apartment
            {
                NumberOfRooms = numOfRooms,
                HasMultipleFloors = hasMultipleFloors,
                DwellingSpaceMin = roomSpace.Item1,
                DwellingSpaceMax = roomSpace.Item2,
                SquareMeterPriceMin = price.Item1,
                SquareMeterPriceMax = price.Item2
            };
        }
    }
}