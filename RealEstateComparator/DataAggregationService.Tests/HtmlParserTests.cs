using DataAggregationService.Aggregators.Common.Services;
using HtmlAgilityPack;
using Xunit;

namespace DataAggregationService.Tests
{
    public class HtmlParserTests
    {
        private const string _htmlSomeNodeLiteral = "<a href=\"/?q=kyiv\" class=\"-selected\" data-search=\"list-item\" data-analytics-click=\"main|geo_search|goto_catalog\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"48\" height=\"48\"><g fill=\"none\" opacity=\".9\"><circle class=\"city-circle\" cx=\"24\" cy=\"24\" r=\"24\" fill=\"none\"></circle><path fill=\"#FFB501\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M7 28l5 13h24l5-13z\"></path><g transform=\"translate(17 4)\"><path fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5.3 2.6C4 2.6 4.1 1.3 3.2 1.3c-1 0-1 1.9.3 3.6.4.6 1.1 1.2 1.6 1.6l.9.1V28H2v3H0v6h14v-6h-2v-3H8V6.7l.5-.1c.5-.3 1.2-1 1.6-1.5 1.3-1.8.9-3.7.1-3.7-1 0-.8 1.3-2 1.3\"></path><path fill=\"#FFBD1A\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5 9v2l1 1h2l1-1V9z\"></path><path fill=\"#C8500A\" d=\"M7 30.9a2 2 0 012 2v4H5v-4c0-1.1.9-2 2-2z\"></path><circle cx=\"6.8\" cy=\"2.4\" r=\"1.5\" fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\"></circle></g></g></svg><div>Київ</div></a>";
        private const string _htmlApartComplexLiteral = "<div class=\"card\"><a href=\"/uk/жк-manhattan-одеса\" class=\"card-media\" data-analytics-click=\"catalog|buildings_list|goto_view_building\" data-impression=\"4707|Catalog Page|1\"><img id=\"card-4707-animation\" class=\"card-image\" src=\"//img.lunstatic.net/building-300x300/38884.jpg\" srcset=\"//img.lunstatic.net/building-600x600/38884.jpg 2x\" alt=\"ЖК Manhattan\"><div class=\"card-label\"><div class=\"card-label-icon\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"13\" viewbox=\"0 0 20 13\" fill=\"#9e9e9e\"><path fill-rule=\"nonzero\" d=\"M10 0C4.48 0 0 2.24 0 5c0 2.24 2.94 4.13 7 4.77V13l4-4-4-4v2.73C3.85 7.17 2 5.83 2 5c0-1.06 3.04-3 8-3s8 1.94 8 3c0 .73-1.46 1.89-4 2.53v2.05c3.53-.77 6-2.53 6-4.58 0-2.76-4.48-5-10-5z\"></path></svg></div></div><div class=\"button -icon favorite\" data-analytics-click=\"catalog|buildings_list|favorites_click\" data-favorites=\"buildings-4707\" data-animation-trigger-for=\"card-4707-animation\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" width=\"24\" height=\"24\" viewbox=\"0 0 24 24\"><defs><path id=\"fav-shadow-outline-path\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09A5.99 5.99 0 0 0 7.5 3 5.45 5.45 0 0 0 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5a3.9 3.9 0 0 1 3.57 2.36h1.87A3.88 3.88 0 0 1 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><filter id=\"fav-shadow\" width=\"170%\" height=\"176.3%\" x=\"-35%\" y=\"-27.2%\" filterunits=\"objectBoundingBox\"><feoffset dy=\"2\" in=\"SourceAlpha\" result=\"shadowOffsetOuter1\"></feoffset><fegaussianblur in=\"shadowOffsetOuter1\" result=\"shadowBlurOuter1\" stddeviation=\"2\"></fegaussianblur><fecolormatrix in=\"shadowBlurOuter1\" values=\"0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.3 0\"></fecolormatrix></filter></defs><g fill-rule=\"evenodd\"><use fill=\"#000\" filter=\"url(#fav-shadow)\" xlink:href=\"#fav-shadow-outline-path\"></use><g><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path fill=\"#fff\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><path d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"></path></g></g></svg></div><div class=\"card-content\"><div class=\"card-location\">Київський</div><div class=\"card-title\">ЖК Manhattan</div></div></a><div class=\"card-content\"><div class=\"card-price\">ід 561 тис. грн                                                </div><div class=\"card-text\">2 будинки будується, 2 підготовчі роботи, 2 в проекті</div><div class=\"card-text \">Graf development</div><a href=\"https://lun.ua/go?placement=card_catalog&amp;building_id=4707&amp;to=https%3A%2F%2Fmht.od.ua%2F\" rel=\"nofollow\" data-analytics-click=\"catalog|buildings_list|goto_away\" target=\"_blank\" class=\"card-action \">mht.od.ua</a><div class=\"card-logo\"><a href=\"/uk/graf-development\" data-analytics-click=\"catalog|buildings_list|goto_view_developer\"><img src=\"//img.lunstatic.net/company-premium/966.svg\" alt=\"Graf development\"></a></div></div></div>";

        private const string _dummyUrl = "https://some.url.com";
        
        [Fact]
        public void ParseText_ReturnText()
        {
            // Arrange
            var htmlNode = HtmlNode.CreateNode(_htmlSomeNodeLiteral);
            const string expectedResult = "Київ";
            var htmlParser = new HtmlParser(new HtmlWeb());
            
            // Act
            var actualResult = htmlParser.ParseText(htmlNode);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ParseHref_ReturnHref()
        {
            // Arrange
            var htmlNode = HtmlNode.CreateNode(_htmlSomeNodeLiteral);
            const string expectedResult = "/?q=kyiv";
            var htmlParser = new HtmlParser(new HtmlWeb());
            
            // Act
            var actualResult = htmlParser.ParseHref(htmlNode);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreateUrl()
        {
            // Arrange
            const string hRef = "/?q=kyiv";
            const string expectedResult = _dummyUrl + hRef;
            var htmlParser = new HtmlParser(new HtmlWeb());
            
            // Act
            var actualResult = htmlParser.CreateUrl(_dummyUrl, hRef);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ParseHrefByXPath()
        {
            // Arrange
            const string apartComplexHRefXPath = ".//a";
            var apartComplexNode = HtmlNode.CreateNode(_htmlApartComplexLiteral);
            const string expectedResult = "/uk/жк-manhattan-одеса";
            
            var htmlParser = new HtmlParser(new HtmlWeb());
            
            // Act
            var actualResult = htmlParser.ParseHrefByXPath(apartComplexNode, apartComplexHRefXPath);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ParseTextByXPath()
        {
            // Arrange
            const string apartComplexNameXPath = ".//a/div[3]/div[@class='card-title']";
            var apartComplexNode = HtmlNode.CreateNode(_htmlApartComplexLiteral);
            const string expectedResult = "ЖК Manhattan";
            
            var htmlParser = new HtmlParser(new HtmlWeb());
            
            // Act
            var actualResult = htmlParser.ParseTextByXPath(apartComplexNode, apartComplexNameXPath);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Theory]
        [InlineData(" data With Spaces ", "dataWithSpaces")]
        [InlineData("dataWithSpaces", "dataWithSpaces")]
        public void RemoveSpaces(string dataWithSpaces, string expectedResult)
        {
            // Arrange
            var htmlParser = new HtmlParser(new HtmlWeb());

            // Act
            var actualResult = htmlParser.RemoveSpaces(dataWithSpaces);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}