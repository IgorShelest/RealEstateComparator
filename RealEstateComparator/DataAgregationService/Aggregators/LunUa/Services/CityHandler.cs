using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.Common;
using DataAgregationService.Aggregators.LunUa;
using HtmlAgilityPack;

namespace DataAgregationService.Agregators.LunUa.Services
{
    public class CityHandler
    {
        private readonly PageHandler _pageHandler;
        private readonly HtmlParser _htmlParser;

        public CityHandler(PageHandler pageHandler, HtmlParser htmlParser)
        {
            _pageHandler = pageHandler;
            _htmlParser = htmlParser;
        }
     
        public async Task<IEnumerable<CityData>> GetCityData()
        {
            var cityHtml = await _pageHandler.LoadCityHtml();
            var cityData = CreateCityData(cityHtml);
            return cityData;
        }
        
        private IEnumerable<CityData> CreateCityData(HtmlNodeCollection cityHtml)
        {
            var cityData = cityHtml?.Select(node =>
                new CityData()
                {
                    Name = _htmlParser.ParseText(node),
                    Url = _pageHandler.CreateLunUaUrl(_htmlParser.ParseHref(node))
                }
            );

            return cityData;
        }
    }
}