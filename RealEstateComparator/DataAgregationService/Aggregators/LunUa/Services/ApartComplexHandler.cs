using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.Common;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.LunUa.Services
{
    public class ApartComplexHandler
    {
        private readonly PageHandler _pageHandler;
        private readonly HtmlParser _htmlParser;

        private const string _source = "LunUa";
        
        public ApartComplexHandler(PageHandler pageHandler, HtmlParser htmlParser)
        {
            _pageHandler = pageHandler;
            _htmlParser = htmlParser;
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
        
        private async Task<ApartComplexesGroupData> GetApartComplexData(CityData cityData)
        {
            var apartComplexGroupHtml = await _pageHandler.LoadApartComplexDataHtml(cityData.Url);
            var apartComplexData = CreateApartComplexData(cityData, apartComplexGroupHtml);
            
            return apartComplexData;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesGroupData apartComplexesGroupData)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerPage = new List<ApartComplex>();

            do
            {
                currentPageUrl = _pageHandler.CreatePageUrl(apartComplexesGroupData.Url, pageNumber++);
                apartComplexesPerPage.AddRange(await GetApartComplexesPerPage(currentPageUrl,
                    apartComplexesGroupData.CityName));
            } while (await _pageHandler.NextPageExists(currentPageUrl));

            return apartComplexesPerPage;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesPerPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtml = await _pageHandler.LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = apartComplexesHtml
                    .Select(complexHtml => CreateApartComplex(complexHtml, cityName));

                return apartComplexes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private ApartComplexesGroupData CreateApartComplexData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesGroupData
            {
                CityName = cityData.Name,
                Url = _pageHandler.CreateLunUaUrl(_htmlParser.ParseHref(parsedApartComplexGroupData))
            };
        }

        private ApartComplex CreateApartComplex(HtmlNode complexHtml, string cityName)
        {
            var temp = new ApartComplex()
            {
                Source = _source,
                Name = _pageHandler.ParseApartComplexText(complexHtml),
                CityName = cityName,
                Url = _pageHandler.CreateLunUaUrl(_pageHandler.ParseApartComplexHRef(complexHtml))
            };

            return temp;
        }
    }
}