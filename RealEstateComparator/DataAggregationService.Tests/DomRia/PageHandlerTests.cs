using System;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.DomRia.Data;
using DataAggregationService.Aggregators.DomRia.Services;
using HtmlAgilityPack;
using Moq;
using Xunit;

namespace DataAggregationService.Tests.DomRia
{
    public class PageHandlerTests
    {
        private const string _homePageUrl = "https://dom.ria.com";
        private const string _dummyUrl = "https://some.url.com";
        
        private const string _htmlApartComplexLiteral = "<section class=\"sc-14ev19b-1 chlTiZ span4\"><div class=\"sc-14ev19b-0 chdwEQ\"><a class=\"photo-298x198  \" title=\"ЖК Cузір&#x27;я-2019\" data-tm=\"catalog-picture\" href=\"/novostroyka-zhk-cuziria-2019-7001/\"><span class=\"load-photo loaded\"><img class=\"\" src=\"https://cdn.riastatic.com/photosnewr/dom/newbuild_photo/ZhK-Cuziria-2019-Kropyvnytskyi-photo__122981-298x198x80.jpg\" title=\"ЖК Cузір&#x27;я-2019\" alt=\"ЖК Cузір&#x27;я-2019\"></span><span class=\"rjwrzt-1 gYgrZ top\"></span><span class=\"rjwrzt-1 gYgrZ\"></span></a><div class=\"sc-14ev19b-2 chupNi\"><h2 class=\"overflowed large seo\"><a class=\"size18\" title=\"ЖК Cузір&#x27;я-2019\" data-tm=\"catalog-name\" href=\"/novostroyka-zhk-cuziria-2019-7001/\">ЖК Cузір&#x27;я-2019</a></h2><div class=\"overflowed large p_wrap size13\">от <span class=\"bold green size18\">14 000</span> <span class=\"green\">грн</span> <!-- -->за м²</div><div class=\"overflowed large size13 mt-5 \"> р‑н Ковалёвка</div><div></div><div class=\"h50\"><div class=\"overflowed large mt-5 mb-5 BuildStatus\"><svg width=\"16\" height=\"16\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\" viewbox=\"0 0 16 16\" class=\"fjfgq8-0 beqDdT\"><path d=\"M3 8l3 3 7-7\" stroke=\"#3c9806\" stroke-width=\"2.5\"></path></svg> <span class=\"bold size13\">Объект сдан</span></div><div class=\"overflowed large mt-5 mb-5\"></div></div><div class=\"sc-14ev19b-3 chCMrr\"><input type=\"checkbox\" data-tm=\"catalog-compare-informer\" id=\"compare-7001\"><a class=\"refine-search\" title=\"Смотреть планировки\" data-tm=\"catalog-planings\" href=\"/novostroyka-zhk-cuziria-2019-planirovki-7001/\">Смотреть планировки →</a><label class=\"compareWrap pointer fl-r\" title=\"Добавить к сравнению\" for=\"compare-7001\"><svg class=\"sukenf-0 ghIKRd compareIcon\" data-tm=\"compareIcon\" width=\"48\" height=\"32\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\" viewbox=\"0 0 48 32\"><path d=\"M.5 3A2.5 2.5 0 013 .5h42A2.5 2.5 0 0147.5 3v26a2.5 2.5 0 01-2.5 2.5H3A2.5 2.5 0 01.5 29V3z\" fill=\"inherit\" stroke=\"#256799\"></path><path class=\"i\" fill-rule=\"evenodd\" clip-rule=\"evenodd\" d=\"M22 9c0-1.1.9-2 2-2s2 .9 2 2h7l4 10h-1l-3.6-9h-.8L28 19h-1l3.6-9H17.4l3.6 9h-1l-3.6-9h-.8L12 19h-1l4-10h7zm2-1a1 1 0 011 1h-2a1 1 0 011-1zM11 20c0 .343.033.678.096 1 .453 2.306 2.447 4 4.904 4s4.451-1.694 4.904-4c.063-.322.096-.657.096-1H11zm8.88 1c-.432 1.747-1.979 3-3.88 3-1.901 0-3.448-1.253-3.88-3h7.76zM27.096 21A5.194 5.194 0 0127 20h10c0 .343-.033.678-.096 1-.453 2.306-2.447 4-4.904 4s-4.451-1.694-4.904-4zM32 24c1.901 0 3.448-1.253 3.88-3h-7.76c.432 1.747 1.979 3 3.88 3z\" fill=\"inherit\"></path></svg></label></div></div></div><div style=\"height:0\" class=\"lazyload-placeholder\"></div></section>";
        private const string _htmlPageActiveLiteral = "<span class=\"page-item\"><span class=\"page-link active\" title=\"1\">1</span></span>";
        private const string _htmlPageInactiveLiteral = "<span class=\"page-item\"><a class=\"page-link\" title=\"17\" href=\"/novostroyki/obl-kievskaya/?page=17\">17</a></span>";
        private const string _htmlDummyNodeLiteral = "<a href=\"/?q=kyiv\" class=\"-selected\" data-search=\"list-item\" data-analytics-click=\"main|geo_search|goto_catalog\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"48\" height=\"48\"><g fill=\"none\" opacity=\".9\"><circle class=\"city-circle\" cx=\"24\" cy=\"24\" r=\"24\" fill=\"none\"></circle><path fill=\"#FFB501\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M7 28l5 13h24l5-13z\"></path><g transform=\"translate(17 4)\"><path fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5.3 2.6C4 2.6 4.1 1.3 3.2 1.3c-1 0-1 1.9.3 3.6.4.6 1.1 1.2 1.6 1.6l.9.1V28H2v3H0v6h14v-6h-2v-3H8V6.7l.5-.1c.5-.3 1.2-1 1.6-1.5 1.3-1.8.9-3.7.1-3.7-1 0-.8 1.3-2 1.3\"></path><path fill=\"#FFBD1A\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5 9v2l1 1h2l1-1V9z\"></path><path fill=\"#C8500A\" d=\"M7 30.9a2 2 0 012 2v4H5v-4c0-1.1.9-2 2-2z\"></path><circle cx=\"6.8\" cy=\"2.4\" r=\"1.5\" fill=\"#FFF\" stroke=\"#C8500A\" stroke-linecap=\"round\" stroke-linejoin=\"round\"></circle></g></g></svg><div>Київ</div></a>";

        private const string _pageNumberXPath = "//*[@id='pagination']/div/div[1]/div/span[*]";
        private const string _apartComplexGroupXPath = "//*[@id='app']/div[3]/div/div[3]/div[6]/ul[17]/li[*]/a";
        private const string _apartComplexXPath = "//*[@id='newbuilds']/section[*]";
        private const string _apartmentXPath = "//*[@id='pricesBlock']/section/div[3]/table/tbody/tr[*]";
        private const string _htmlApartmentThreeRoomLiteral = "<tr class=\"sc-4h0zkt-0 cUukBF plansRow\"><td class=\"iefrfq-0 IasTm\"><a data-tm=\"plans_link_3k\" href=\"/novostroyka-zhk-solnechnaia-ryvera-planirovki-4232/?filterRelease=2020_2&amp;filterRoomCount=3k\">3-комнатные (1)</a></td><td data-tm=\"plans_area_3k\" class=\"iefrfq-0 IasTm\"><span>от <b>93</b> м²</span></td><td class=\"iefrfq-0 IasTm text-r\" data-tm=\"plans_price_3k\"><span>от <span class=\"green\"><b>4 614 009</b> грн</span></span></td></tr>";
        private const string _htmlApartmentMultipleFloorsLiteral = "<tr class=\"sc-4h0zkt-0 cUukBF plansRow\"><td class=\"iefrfq-0 IasTm\"><a data-tm=\"plans_link_multi-level\" href=\"/novostroyka-zhk-lvovskaia-ploshchad-planirovki-6177/?filterRoomCount=multi-level\">Много­уровневые (15)</a></td><td data-tm=\"plans_area_multi-level\" class=\"iefrfq-0 IasTm\"><span>от <b>87.45</b> м²</span></td><td class=\"iefrfq-0 IasTm text-r\" data-tm=\"plans_price_multi-level\"><span>от <span class=\"green\"><b>4 310 498</b> грн</span></span></td></tr>";
        
        [Fact]
        public void CreateUrl_ReturnExpectedString()
        {
            // Arrange
            var pageHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            const string hRef = "/uk/жк-oasis-київ";
            const string expectedResult = _homePageUrl + hRef;
            
            // Act
            var actualResult = pageHandler.CreateDomRiaUrl(hRef);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreatePageUrl_ReturnExpectedString()
        {
            // Arrange
            var pageHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            const string pageTag = "?page=";
            const int pageNumber = 7;
            var expectedResult = _dummyUrl + pageTag + pageNumber;
            
            // Act
            var actualResult = pageHandler.CreatePageUrl(_dummyUrl, pageNumber);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ParseApartComplexText_ReturnApartComplexText()
        {
            // Arrange
            const string apartComplexNameXPath = ".//div/div/h2/a";
            var apartComplexNode = HtmlNode.CreateNode(_htmlApartComplexLiteral);
            const string expectedResult = "ЖК Manhattan";
            
            var htmlParserMock = new Mock<HtmlParser>(new HtmlWeb());
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseTextByXPath(apartComplexNode, apartComplexNameXPath))
                .Returns(expectedResult);
            
            var pageHandler = new PageHandler(htmlParserMock.Object);
            
            // Act
            var actualResult = pageHandler.ParseApartComplexText(apartComplexNode);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ParseApartComplexHRef_ReturnApartComplexHRef()
        {
            // Arrange
            const string apartComplexNameXPath = ".//div/div/h2/a";
            var apartComplexNode = HtmlNode.CreateNode(_htmlApartComplexLiteral);
            const string expectedResult = "/uk/жк-manhattan-одеса";
            
            var htmlParserMock = new Mock<HtmlParser>(new HtmlWeb());
            htmlParserMock
                .Setup(htmlParser => htmlParser.ParseHrefByXPath(apartComplexNode, apartComplexNameXPath))
                .Returns(expectedResult);
            
            var pageHandler = new PageHandler(htmlParserMock.Object);
            
            // Act
            var actualResult = pageHandler.ParseApartComplexHRef(apartComplexNode);
        
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
       
        [Fact]
        public async Task NextPageExists_ReturnTrue()
        {
            // Arrange
            var loadHtmlNodesTask = Task.Run(() => new HtmlNodeCollection(null)
            {
                HtmlNode.CreateNode(_htmlPageActiveLiteral),
                HtmlNode.CreateNode(_htmlPageInactiveLiteral)
            });
            
            var htmlParser = new Mock<HtmlParser>(new HtmlWeb());
            htmlParser
                .Setup(htmlParser => htmlParser.LoadHtmlNodes(_dummyUrl, _pageNumberXPath))
                .Returns(loadHtmlNodesTask);
            
            const bool expectedResult = true;
            var pageHandler = new PageHandler(htmlParser.Object);

            // Act
            var actualResult = await pageHandler.NextPageExists(_dummyUrl);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public async Task NextPageExists_ReturnFalse()
        {
            // Arrange
            var loadHtmlNodesTask = Task.Run(() => new HtmlNodeCollection(null)
            {
                HtmlNode.CreateNode(_htmlPageInactiveLiteral),
                HtmlNode.CreateNode(_htmlPageActiveLiteral)

            });
            
            var htmlParser = new Mock<HtmlParser>(new HtmlWeb());
            htmlParser
                .Setup(htmlParser => htmlParser.LoadHtmlNodes(_dummyUrl, _pageNumberXPath))
                .Returns(loadHtmlNodesTask);
            
            const bool expectedResult = false;
            var pageHandler = new PageHandler(htmlParser.Object);

            // Act
            var actualResult = await pageHandler.NextPageExists(_dummyUrl);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public async Task LoadApartComplexDataHtml()
        {
            // Arrange
            var apartComplexHtmlNode = HtmlNode.CreateNode(_htmlApartComplexLiteral);
            var loadHtmlNodesTask = Task.Run(() => new HtmlNodeCollection(null)
            {
                apartComplexHtmlNode
            });    
            
            var htmlParser = new Mock<HtmlParser>(new HtmlWeb());
            htmlParser
                .Setup(htmlParser => htmlParser.LoadHtmlNodes(_homePageUrl, _apartComplexGroupXPath))
                .Returns(loadHtmlNodesTask);
            
            var expectedResult = new HtmlNodeCollection(null)
            {
                apartComplexHtmlNode
            };
            var pageHandler = new PageHandler(htmlParser.Object);

            // Act
            var actualResult = await pageHandler.LoadApartComplexDataHtml();
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task LoadApartComplexesHtml()
        {
            // Arrange
            var expectedResult = new HtmlNodeCollection(null)
            {
                HtmlNode.CreateNode(_htmlDummyNodeLiteral),
                HtmlNode.CreateNode(_htmlDummyNodeLiteral)
            };
            
            var loadHtmlNodesTask = Task.Run(() => expectedResult);    
            
            var htmlParser = new Mock<HtmlParser>(new HtmlWeb());
            htmlParser
                .Setup(htmlParser => htmlParser.LoadHtmlNodes(_dummyUrl, _apartComplexXPath))
                .Returns(loadHtmlNodesTask);
            
            var pageHandler = new PageHandler(htmlParser.Object);

            // Act
            var actualResult = await pageHandler.LoadApartComplexesHtml(_dummyUrl);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task LoadApartmentsHtml()
        {
            // Arrange
            var expectedResult = new HtmlNodeCollection(null)
            {
                HtmlNode.CreateNode(_htmlDummyNodeLiteral),
                HtmlNode.CreateNode(_htmlDummyNodeLiteral)
            };
            
            var loadHtmlNodesTask = Task.Run(() => expectedResult);    
            
            var htmlParser = new Mock<HtmlParser>(new HtmlWeb());
            htmlParser
                .Setup(htmlParser => htmlParser.LoadHtmlNodes(_dummyUrl, _apartmentXPath))
                .Returns(loadHtmlNodesTask);
            
            var pageHandler = new PageHandler(htmlParser.Object);

            // Act
            var actualResult = await pageHandler.LoadApartmentsHtml(_dummyUrl);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void ParseHtmlNumOfRooms()
        {
            // Arrange
            var apartmentNode = HtmlNode.CreateNode(_htmlApartmentThreeRoomLiteral);
            var htmlHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            const int expectedResult = 3;
            
            // Act
            var actualResult = htmlHandler.ParseHtmlNumOfRooms(apartmentNode);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
 
        [Theory]
        [InlineData(_htmlApartmentThreeRoomLiteral, false)]
        [InlineData(_htmlApartmentMultipleFloorsLiteral, true)]
        public void HasMultipleFloors(string apartmentHtmlNodeLiteral, bool expectedResult)
        {
            // Arrange
            var apartmentNode = HtmlNode.CreateNode(apartmentHtmlNodeLiteral);
            var htmlHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            
            // Act
            var actualResult = htmlHandler.HasMultipleFloors(apartmentNode);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(0, 93)]
        [InlineData(120, 120)]
        public void ParseHtmlRoomSpace(int previousSpace, int maxSpace)
        {
            // Arrange
            var expectedResult = new Tuple<int, int>(93, maxSpace);
            var apartmentNode = HtmlNode.CreateNode(_htmlApartmentThreeRoomLiteral);
            var transferData = new ApartmentTransferData()
            {
                PreviousSpace = previousSpace
            };
            var htmlHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            
            // Act
            var actualResult = htmlHandler.ParseHtmlRoomSpace(apartmentNode, ref transferData);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(0, 4614009)]
        [InlineData(5614009, 5614009)]
        public void ParseHtmlApartPrice(int previousPrice, int maxPrice)
        {
            // Arrange
            var expectedResult = new Tuple<int, int>(4614009, maxPrice);
            var apartmentNode = HtmlNode.CreateNode(_htmlApartmentThreeRoomLiteral);
            var transferData = new ApartmentTransferData()
            {
                PreviousPrice = previousPrice
            };
            var htmlHandler = new PageHandler(new HtmlParser(new HtmlWeb()));
            
            // Act
            var actualResult = htmlHandler.ParseHtmlApartPrice(apartmentNode, ref transferData);
            
            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}