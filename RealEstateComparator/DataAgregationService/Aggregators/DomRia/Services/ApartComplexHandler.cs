using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common;
using DataAggregationService.Aggregators.Common.Services;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.DomRia.Services
{
    public class ApartComplexHandler
    {
        private readonly PageHandler _pageHandler;
        private readonly HtmlParser _htmlParser;
        private const string _source = "DomRia";

        public ApartComplexHandler(PageHandler pageHandler, HtmlParser htmlParser)
        {
            _pageHandler = pageHandler;
            _htmlParser = htmlParser;
        }
        
        public async Task<IEnumerable<ApartComplex>> GetApartComplexes()
        {
            var apartComplexData = await GetApartComplexData();
            var apartComplexesForAllCitiesTasks = apartComplexData.Select(GetApartComplexesPerCity);
            var apartComplexesForAllCities = await Task.WhenAll(apartComplexesForAllCitiesTasks);
            
            var combinedResults = new List<ApartComplex>();
            apartComplexesForAllCities.ToList().ForEach(apartments => combinedResults.AddRange(apartments));
            
            return combinedResults;
        }
        
        private async Task<IEnumerable<ApartComplexesGroupData>> GetApartComplexData()
        {
            var apartComplexHtml = await _pageHandler.LoadApartComplexDataHtml();
            var apartComplexData = CreateApartComplexData(apartComplexHtml);

            return apartComplexData;
        }

        private IEnumerable<ApartComplexesGroupData> CreateApartComplexData(HtmlNodeCollection cityHtml)
        {
            var apartComplexesData = cityHtml?.Select(node =>
                new ApartComplexesGroupData()
                {
                    CityName = _htmlParser.ParseText(node),                        
                    Url = _pageHandler.CreateDomRiaUrl(_htmlParser.ParseHref(node))
                }
            );

            return apartComplexesData;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesPerCity(ApartComplexesGroupData apartComplexGroupData)
        {
            try
            {
                var apartComplexesPerCity = await GetApartComplexesForAllPages(apartComplexGroupData);

                return apartComplexesPerCity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesGroupData apartComplexesGroupData)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerCity = new List<ApartComplex>();

            do
            {
                currentPageUrl = _pageHandler.CreatePageUrl(apartComplexesGroupData.Url, pageNumber++);
                var apartComplexesPerPage = await GetApartComplexesPerPage(currentPageUrl, apartComplexesGroupData.CityName);
                apartComplexesPerCity.AddRange(apartComplexesPerPage);
            } while (await _pageHandler.NextPageExists(currentPageUrl));

            return apartComplexesPerCity;
        }
        
        private async Task<IEnumerable<ApartComplex>> GetApartComplexesPerPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtml =  await _pageHandler.LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = apartComplexesHtml.Select(complex => CreateApartComplex(complex, cityName));
                
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
                Name = _pageHandler.ParseApartComplexText(complex),
                CityName = cityName,
                Url = _pageHandler.CreateDomRiaUrl(_pageHandler.ParseApartComplexHRef(complex))
            };
        }
    }
}