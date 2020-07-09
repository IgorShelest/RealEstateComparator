using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.LunUa.Services;
using HtmlAgilityPack;
using Moq;
using Xunit;

namespace DataAggregationService.Tests
{
    public class LunUaCityHandler
    {
        private const string _htmlCity1Literal = "<a href=\"/?q=kyiv\" class=\"-selected\" data-search=\"list-item\" data-analytics-click=\"main|geo_search|goto_catalog\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"48\" height=\"48\"><g fill=\"none\" opacity=\".9\"><circle class=\"city-circle\" cx=\"24\" cy=\"24\" r=\"24\" fill=\"none\"></circle><path fill=\"#FFB501\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M7 28l5 13h24l5-13z\"></path><g transform=\"translate(17 4)\"><path fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5.3 2.6C4 2.6 4.1 1.3 3.2 1.3c-1 0-1 1.9.3 3.6.4.6 1.1 1.2 1.6 1.6l.9.1V28H2v3H0v6h14v-6h-2v-3H8V6.7l.5-.1c.5-.3 1.2-1 1.6-1.5 1.3-1.8.9-3.7.1-3.7-1 0-.8 1.3-2 1.3\"></path><path fill=\"#FFBD1A\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5 9v2l1 1h2l1-1V9z\"></path><path fill=\"#C8500A\" d=\"M7 30.9a2 2 0 012 2v4H5v-4c0-1.1.9-2 2-2z\"></path><circle cx=\"6.8\" cy=\"2.4\" r=\"1.5\" fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\"></circle></g></g></svg><div>Київ</div></a>";
        private const string _htmlCity2Literal = "<a href=\"/?q=kyiv_region\" data-search=\"list-item\" data-analytics-click=\"main|geo_search|goto_catalog\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"48\" height=\"48\"><g fill=\"none\" fill-rule=\"evenodd\" opacity=\".9\"><circle class=\"city-circle\" cx=\"24\" cy=\"24\" r=\"24\" fill=\"none\"></circle><path fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M10.6 12.5h15.7v24H10.6z\"></path><path fill=\"#C8500A\" d=\"M17 18.5h3v3h-3zm0 9h3v3h-3z\"></path><g stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" transform=\"translate(27 21)\"><circle cx=\"6.6\" cy=\"6.6\" r=\"5.7\" fill=\"#FFB501\"></circle><path d=\"M6.7 15.3V1.4M9 8.2l-2.3 2.2M4 5.1L6.7 8\"></path></g><path fill=\"#FFB501\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9.1 11.2H28v2.9H9.1z\"></path></g></svg><div>Київська обл.</div></a>";

        [Fact]
        public async Task GetCityData()
        {
            // Arrange
            var city1Node = HtmlNode.CreateNode(_htmlCity1Literal);
            var city2Node = HtmlNode.CreateNode(_htmlCity2Literal);
            
            var expectedResult = new List<CityData>
            {
                new CityData() {Name = "Київ", Url = "https://lun.ua/?q=kyiv"},
                new CityData() {Name = "Київська обл.", Url = "https://lun.ua/?q=kyiv_region"}
            };

            var cityHRefs = new List<string>
            {
                "/?q=kyiv", "/?q=kyiv_region"
            };
            
            var htmlWebMock = new Mock<HtmlWeb>();
            var htmlParserMock = new Mock<HtmlParser>(htmlWebMock.Object);
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseText(city1Node))
                .Returns(expectedResult[0].Name);
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseText(city2Node))
                .Returns(expectedResult[1].Name);
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseHref(city1Node))
                .Returns(cityHRefs[0]);
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseHref(city2Node))
                .Returns(cityHRefs[1]);
            
            var pageHandlerMock = new Mock<PageHandler>(htmlParserMock.Object);
            pageHandlerMock
                .Setup(pageHandler => pageHandler.LoadCityHtml())
                .ReturnsAsync(new HtmlNodeCollection(null)
                {
                    city1Node, city2Node
                });
            pageHandlerMock
                .Setup(pageHandler => pageHandler.CreateLunUaUrl(cityHRefs[0]))
                .Returns(expectedResult[0].Url);
            pageHandlerMock
                .Setup(pageHandler => pageHandler.CreateLunUaUrl(cityHRefs[1]))
                .Returns(expectedResult[1].Url);

            var cityHandler = new CityHandler(pageHandlerMock.Object, htmlParserMock.Object);
            
            // Act
            var actualResult = await cityHandler.GetCityData();
            
            // Assert
            Assert.Equal(expectedResult.ToList(), actualResult.ToList());
        }
    }
}