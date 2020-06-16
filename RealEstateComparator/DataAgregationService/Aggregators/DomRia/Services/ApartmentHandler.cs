using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Parsers.DomRia.Data;
using HtmlAgilityPack;

namespace DataAggregationService.Parsers.DomRia.Services
{
    public class ApartmentHandler
    {
        private readonly PageHandler _pageHandler;

        public ApartmentHandler()
        {
            _pageHandler = new PageHandler();
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
                return CreateApartmentsPerApartComplex(htmlNodes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private IEnumerable<Apartment> CreateApartmentsPerApartComplex(HtmlNodeCollection htmlNodes)
        {
            var transferData = new ApartmentTransferData();
            return htmlNodes?.Reverse().Select(node => CreateApartment(node, ref transferData)).ToList();
        }

        private Apartment CreateApartment(HtmlNode node, ref ApartmentTransferData transferData)
        {
            var numOfRooms = _pageHandler.ParseHtmlNumOfRooms(node);
            var hasMultipleFloors = _pageHandler.HasMultipleFloors(node);
            var roomSpace = PageHandler.ParseHtmlRoomSpace(node, ref transferData);
            var price = _pageHandler.ParseHtmlApartPrice(node, ref transferData);

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