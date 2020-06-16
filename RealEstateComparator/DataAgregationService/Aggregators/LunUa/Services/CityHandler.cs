using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Parsers.Common;
using HtmlAgilityPack;

namespace DataAgregationService.Parsers.LunUa
{
    public class CityHandler
    {
        private readonly PageHandler _pageHandler;
        private readonly HtmlParser _htmlParser;

        public CityHandler()
        {
            _pageHandler = new PageHandler();
            _htmlParser = new HtmlParser();
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