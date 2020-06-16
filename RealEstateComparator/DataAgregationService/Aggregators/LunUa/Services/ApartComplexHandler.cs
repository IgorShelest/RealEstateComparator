using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Parsers.Common;
using HtmlAgilityPack;

namespace DataAgregationService.Parsers.LunUa
{
    public class ApartComplexHandler
    {
        private readonly PageHandler _pageHandler;
        private readonly HtmlParser _htmlParser;

        private const string _source = "LunUa";
        
        public ApartComplexHandler()
        {
            _pageHandler = new PageHandler();
            _htmlParser = new HtmlParser();
        }
        
        public async Task<IEnumerable<ApartComplex>> GetApartComplexes(IEnumerable<CityData> allCitiesData)
        {
            var apartComplexesForAllCitiesTasks = allCitiesData.Select(GetApartComplexesPerCity);
            var apartComplexesForAllCities = await Task.WhenAll(apartComplexesForAllCitiesTasks);

            var combinedResults = new List<ApartComplex>();
            apartComplexesForAllCities.ToList().ForEach(apartments => combinedResults.AddRange(apartments));
            
            return combinedResults;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesPerCity(CityData cityData)
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
        
        private async Task<ApartComplexesDataPerCity> GetApartComplexData(CityData cityData)
        {
            var apartComplexGroupHtml = await _pageHandler.LoadApartComplexDataHtml(cityData.Url);
            var apartComplexData = CreateApartComplexData(cityData, apartComplexGroupHtml);
            
            return apartComplexData;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesDataPerCity apartComplexesDataPerCity)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerPage = new List<ApartComplex>();

            do
            {
                currentPageUrl = _pageHandler.CreatePageUrl(apartComplexesDataPerCity.Url, pageNumber++);
                apartComplexesPerPage.AddRange(await GetApartComplexesPerPage(currentPageUrl, apartComplexesDataPerCity.CityName));
            }
            while (false); // */(await _pageHandler.NextPageExists(currentPageUrl));

            return apartComplexesPerPage;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesPerPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtml = await _pageHandler.LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = apartComplexesHtml
                    .Select(complex => CreateApartComplex(complex, cityName));

                return apartComplexes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private ApartComplexesDataPerCity CreateApartComplexData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesDataPerCity
            {
                CityName = cityData.Name,
                Url = _pageHandler.CreateLunUaUrl(_htmlParser.ParseHref(parsedApartComplexGroupData))
            };
        }

        private ApartComplex CreateApartComplex(HtmlNode complex, string cityName)
        {
            var temp = new ApartComplex()
            {
                Source = _source,
                Name = _pageHandler.ParseApartComplexText(complex),
                CityName = cityName,
                Url = _pageHandler.CreateLunUaUrl(_pageHandler.ParseApartComplexHRef(complex))
            };

            return temp;
        }
    }
}