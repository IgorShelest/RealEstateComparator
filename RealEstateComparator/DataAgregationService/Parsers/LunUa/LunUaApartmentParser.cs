using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Interfaces;
using DataAggregationService.ParsedData.LunUa;
using DataAgregationService.Parsers.LunUa;
using HtmlAgilityPack;

namespace DataAggregationService.Parsers.LunUa
{
    class LunUaApartmentParser : IApartmentParser
    {
        private readonly HtmlHandler _htmlHandler;

        public LunUaApartmentParser()
        {
            _htmlHandler = new HtmlHandler();
        }

        public async Task<IEnumerable<ApartComplex>> GetApartmentData()
        {
            var citiesData = await GetCityData();
            var apartComplexes = await GetApartmentsForAllCities(citiesData);
            return apartComplexes;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartmentsForAllCities(IEnumerable<CityData> allCitiesData)
        {
            var apartComplexesForAllCitiesTasks = allCitiesData.Select(GetApartmentsForOneCity);
            var apartComplexesForAllCities = await Task.WhenAll(apartComplexesForAllCitiesTasks);

            var combinedResults = new List<ApartComplex>();
            apartComplexesForAllCities.ToList().ForEach(apartments => combinedResults.AddRange(apartments));
            
            return combinedResults;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartmentsForOneCity(CityData cityData)
        {
            try
            {
                var apartComplexDataPerCity = await GetApartComplexData(cityData);
                var apartComplexesPerCity = await GetApartComplexesForAllPages(apartComplexDataPerCity);

                return apartComplexesPerCity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<IEnumerable<CityData>> GetCityData()
        {
            var cityHtml = await _htmlHandler.LoadCityHtml();
            var cityData = CreateCityData(cityHtml);
            return cityData;
        }

        private IEnumerable<CityData> CreateCityData(HtmlNodeCollection cityHtml)
        {
            var cityData = cityHtml?.Select(node =>
                new CityData()
                {
                    Name = _htmlHandler.ParseText(node),
                    Url = _htmlHandler.CreateUrl(_htmlHandler.ParseHref(node))
                }
            );

            return cityData;
        }

        private async Task<ApartComplexesDataPerCity> GetApartComplexData(CityData cityData)
        {
            var apartComplexGroupHtml = await _htmlHandler.LoadApartComplexDataHtml(cityData.Url);
            return CreateApartComplexData(cityData, apartComplexGroupHtml);
        }

        private ApartComplexesDataPerCity CreateApartComplexData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesDataPerCity
            {
                CityName = cityData.Name,
                Url = _htmlHandler.CreateUrl(_htmlHandler.ParseHref(parsedApartComplexGroupData))
            };
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesDataPerCity apartComplexesDataPerCity)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerPage = new List<ApartComplex>();

            do
            {
                currentPageUrl = _htmlHandler.CreatePageUrl(apartComplexesDataPerCity.Url, pageNumber++);
                apartComplexesPerPage.AddRange(await GetApartComplexesForPage(currentPageUrl, apartComplexesDataPerCity.CityName));
            }
            while (false); // (NextPageExists(currentPageUrl));

            return apartComplexesPerPage;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtml = await _htmlHandler.LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = CreateApartComplexes(apartComplexesHtml, cityName).ToList();

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

        private async Task<IEnumerable<Apartment>> GetApartmentsForApartComplex(string url)
        {
            try
            {
                var htmlNodes = await _htmlHandler.LoadApartmentsHtml(url);
                var apartments = CreateApartmentsPerApartComplex(htmlNodes);
                return apartments;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private IEnumerable<ApartComplex> CreateApartComplexes(HtmlNodeCollection apartComplexesPerPageHtml, string cityName)
        {
            return apartComplexesPerPageHtml.Select(complex => CreateApartComplex(complex, cityName));
        }

        private ApartComplex CreateApartComplex(HtmlNode complex, string cityName)
        {
            return new ApartComplex()
            {
                Name = _htmlHandler.ParseApartComplexText(complex),
                CityName = cityName,
                Url = _htmlHandler.CreateUrl(_htmlHandler.ParseApartComplexHRef(complex))
            };
        }

        private IEnumerable<Apartment> CreateApartmentsPerApartComplex(HtmlNodeCollection htmlNodes)
        {
            var apartments = htmlNodes?.Select(CreateApartment).ToList();
            return apartments;
        }

        private Apartment CreateApartment(HtmlNode node)
        {
            var numOfRooms = _htmlHandler.ParseHtmlNumOfRooms(node);
            var hasMultipleFloors = _htmlHandler.HasMultipleFloors(node);
            var roomSpace = _htmlHandler.ParseHtmlRoomSpace(node);
            var price = _htmlHandler.ParseHtmlApartPrice(node);

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
