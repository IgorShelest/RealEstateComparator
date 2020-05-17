using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Interfaces;
using DataAggregationService.Parsers.Common;
using DataAggregationService.Parsers.DomRia.Data;
using DataAggregationService.Parsers.DomRia.Services;
using DataAgregationService.Parsers.LunUa;
using HtmlAgilityPack;

namespace DataAggregationService.Parsers.DomRia
{
    public class DomRiaApartmentParser: IApartmentParser
    {
        private readonly HtmlHandlerDomRia _htmlHandler;
        private const string _source = "DomRia";

        public DomRiaApartmentParser()
        {
            _htmlHandler = new HtmlHandlerDomRia();
        }

        public async Task<IEnumerable<ApartComplex>> GetApartmentData()
        {
            var apartComplexData = await GetApartComplexData();
            return await GetApartmentsForAllCities(apartComplexData);
        }

        private async Task<IEnumerable<ApartComplexesDataPerCity>> GetApartComplexData()
        {
            var apartComplexHtml = await _htmlHandler.LoadApartComplexDataHtml();
            return CreateApartComplexData(apartComplexHtml);
        }

        private IEnumerable<ApartComplexesDataPerCity> CreateApartComplexData(HtmlNodeCollection cityHtml)
        {
            var apartComplexesData = cityHtml?.Select(node =>
                new ApartComplexesDataPerCity()
                {
                    CityName = _htmlHandler.ParseText(node),                        
                    Url = _htmlHandler.CreateUrl(_htmlHandler.ParseHref(node))
                }
            );

            return apartComplexesData;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartmentsForAllCities(IEnumerable<ApartComplexesDataPerCity> apartComplexData)
        {
            var apartComplexesForAllCitiesTasks = apartComplexData.Select(GetApartmentsForOneCity);
            var apartComplexesForAllCities = await Task.WhenAll(apartComplexesForAllCitiesTasks);
            
            var combinedResults = new List<ApartComplex>();
            apartComplexesForAllCities.ToList().ForEach(apartments => combinedResults.AddRange(apartments));
            
            return combinedResults;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartmentsForOneCity(ApartComplexesDataPerCity apartComplexDataPerCity)
        {
            try
            {
                var apartComplexesPerCity = await GetApartComplexesForAllPages(apartComplexDataPerCity);

                return apartComplexesPerCity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesDataPerCity apartComplexesDataPerCity)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerCity = new List<ApartComplex>();

            do
            {
                currentPageUrl = _htmlHandler.CreatePageUrl(apartComplexesDataPerCity.Url, pageNumber++);
                var apartComplexesPerPage = await GetApartComplexesForPage(currentPageUrl, apartComplexesDataPerCity.CityName);
                apartComplexesPerCity.AddRange(apartComplexesPerPage);
            } while (false);//await _htmlHandler.NextPageExists(currentPageUrl));

            return apartComplexesPerCity;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtml =  await _htmlHandler.LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = apartComplexesHtml.Select(complex => CreateApartComplex(complex, cityName)).ToList();

                foreach (var complex in apartComplexes)
                    complex.Apartments = await GetApartmentsForApartComplex(complex.Url);
                
                return apartComplexes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private ApartComplex CreateApartComplex(HtmlNode complex, string cityName)
        {
            return new ApartComplex()
            {
                Source = _source,
                Name = _htmlHandler.ParseApartComplexText(complex),
                CityName = cityName,
                Url = _htmlHandler.CreateUrl(_htmlHandler.ParseApartComplexHRef(complex))
            };
        }

        private async Task<IEnumerable<Apartment>> GetApartmentsForApartComplex(string url)
        {
            try
            {
                var htmlNodes = await _htmlHandler.LoadApartmentsHtml(url);
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
            var numOfRooms = _htmlHandler.ParseHtmlNumOfRooms(node);
            var hasMultipleFloors = HtmlHandlerDomRia.HasMultipleFloors(node);
            var roomSpace = HtmlHandlerDomRia.ParseHtmlRoomSpace(node, ref transferData);
            var price = _htmlHandler.ParseHtmlApartPrice(node, ref transferData);

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