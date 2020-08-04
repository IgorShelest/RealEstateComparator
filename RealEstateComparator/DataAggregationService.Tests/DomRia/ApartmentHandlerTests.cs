using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.DomRia.Data;
using DataAggregationService.Aggregators.DomRia.Services;
using HtmlAgilityPack;
using Moq;
using Xunit;

namespace DataAggregationService.Tests.DomRia
{
    public class ApartmentHandlerTests
    {
        [Fact]
        public async Task SetApartments()
        {
            // Arrange
            var apartments = CreateApartments().ToList();
            var apartComplexes = CreateApartComplexes().ToList();

            var htmlWebMock = new Mock<HtmlWeb>();
            var htmlParserMock = new Mock<HtmlParser>(htmlWebMock.Object);
            var pageHandlerMock = MockPageHandler(htmlParserMock, apartments, apartComplexes);
            var apartmentHandler = new ApartmentHandler(pageHandlerMock.Object);
            
            // Act
            await apartmentHandler.SetApartments(apartComplexes);
            
            // Assert
            Assert.True(CompareApartments(apartments[0], apartComplexes[0].Apartments.ToList()[0]));
            Assert.True(CompareApartments(apartments[1], apartComplexes[0].Apartments.ToList()[1]));
            Assert.True(CompareApartments(apartments[2], apartComplexes[1].Apartments.ToList()[0]));
            Assert.True(CompareApartments(apartments[3], apartComplexes[1].Apartments.ToList()[1]));
        }

        private bool CompareApartments(Apartment lhs, Apartment rhs)
        {
            return lhs.NumberOfRooms == rhs.NumberOfRooms
                   && lhs.HasMultipleFloors == rhs.HasMultipleFloors
                   && lhs.DwellingSpaceMin == rhs.DwellingSpaceMin
                   && lhs.DwellingSpaceMax == rhs.DwellingSpaceMax
                   && lhs.SquareMeterPriceMin == rhs.SquareMeterPriceMin
                   && lhs.SquareMeterPriceMax == rhs.SquareMeterPriceMax;
        }

        private Mock<PageHandler> MockPageHandler(Mock<HtmlParser> htmlParserMock, IEnumerable<Apartment> apartments, IEnumerable<ApartComplex> apartComplexes)
        {
            var htmlApartmentNodes = CreateApartmentNodes();
            var pageHandlerMock = new Mock<PageHandler>(htmlParserMock.Object);
            
            MockLoadApartmentsHtml(pageHandlerMock, htmlApartmentNodes, apartComplexes);
            MockParseHtmlNumOfRooms(pageHandlerMock, htmlApartmentNodes, apartments);
            MockHasMultipleFloors(pageHandlerMock, htmlApartmentNodes, apartments);
            MockParseHtmlRoomSpace(pageHandlerMock, htmlApartmentNodes, apartments);
            MockParseHtmlApartPrice(pageHandlerMock, htmlApartmentNodes, apartments);

            return pageHandlerMock;
        }

        private void MockLoadApartmentsHtml(Mock<PageHandler> pageHandlerMock, IEnumerable<HtmlNode> htmlApartmentNodesInput, IEnumerable<ApartComplex> apartComplexes)
        {
            var htmlApartmentNodes = htmlApartmentNodesInput.ToList();
            
            pageHandlerMock
                .Setup(pageHandler => pageHandler.LoadApartmentsHtml(apartComplexes.ToList()[0].Url))
                .ReturnsAsync(new HtmlNodeCollection(null)
                {
                    htmlApartmentNodes[1],
                    htmlApartmentNodes[0]
                });
            
            pageHandlerMock
                .Setup(pageHandler => pageHandler.LoadApartmentsHtml(apartComplexes.ToList()[1].Url))
                .ReturnsAsync(new HtmlNodeCollection(null)
                {
                    htmlApartmentNodes[3],
                    htmlApartmentNodes[2]
                });
        }

        private void MockParseHtmlNumOfRooms(Mock<PageHandler> pageHandlerMock, IEnumerable<HtmlNode> apartmentNodesInput, IEnumerable<Apartment> apartmentsInput)
        {
            var apartmentNodes = apartmentNodesInput.ToList();
            var apartments = apartmentsInput.ToList();
            
            for (var iter = 0; iter < apartmentNodes.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.ParseHtmlNumOfRooms(apartmentNodes[iter]))
                    .Returns(apartments[iter].NumberOfRooms);
            }
        }

        private void MockHasMultipleFloors(Mock<PageHandler> pageHandlerMock, IEnumerable<HtmlNode> htmlApartmentNodesInput, IEnumerable<Apartment> expectedResultInput)
        {
            var htmlApartmentNodes = htmlApartmentNodesInput.ToList();
            var expectedResult = expectedResultInput.ToList();
            
            for (var iter = 0; iter < htmlApartmentNodes.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.HasMultipleFloors(htmlApartmentNodes[iter]))
                    .Returns(expectedResult[iter].HasMultipleFloors);
            }
        }

        private void MockParseHtmlRoomSpace(Mock<PageHandler> pageHandlerMock, IEnumerable<HtmlNode> htmlApartmentNodesInput, IEnumerable<Apartment> expectedResultInput)
        {
            var htmlApartmentNodes = htmlApartmentNodesInput.ToList();
            var apartments = expectedResultInput.ToList();
            
            for (var iter = 0; iter < htmlApartmentNodes.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.ParseHtmlRoomSpace(htmlApartmentNodes[iter], ref It.Ref<ApartmentTransferData>.IsAny))
                    .Returns(new Tuple<int, int>(apartments[iter].DwellingSpaceMin, apartments[iter].DwellingSpaceMax));
            }
        }

        private void MockParseHtmlApartPrice(Mock<PageHandler> pageHandlerMock, IEnumerable<HtmlNode> htmlApartmentNodesInput, IEnumerable<Apartment> expectedResultInput)
        {
            var htmlApartmentNodes = htmlApartmentNodesInput.ToList();
            var apartments = expectedResultInput.ToList();
            
            for (var iter = 0; iter < htmlApartmentNodes.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.ParseHtmlApartPrice(htmlApartmentNodes[iter], ref It.Ref<ApartmentTransferData>.IsAny))
                    .Returns(new Tuple<int, int>(apartments[iter].SquareMeterPriceMin, apartments[iter].SquareMeterPriceMax));
            }
        }

        private IEnumerable<HtmlNode> CreateApartmentNodes()
        {
            const string htmlApartment1Literal = "<a href=\"/uk/жк-лебединий-київ/планування/двокімнатні\" class=\"BuildingPrices-row\" data-analytics-click=\"view_building_about|prices|goto_layout_view\"><div class=\"BuildingPrices-cell -img\"><img class=\"BuildingPrices-image lazyload\" data-src=\"//img.lunstatic.net/vector-layout/42189-1.svg\" alt=\"ЖК Лебединий: планування 2-кімнатної квартири 64.5 м2, тип 2Б\"></div><div class=\"BuildingPrices-subrow\"><div class=\"BuildingPrices-cell\">2-кімнатні</div><div class=\"BuildingPrices-cell\">ід                <span data-currency=\"uah\" class=\"\">1.49 млн грн</span><span data-currency=\"usd\" class=\"hidden\">55 720 $</span></div></div><div class=\"BuildingPrices-subrow \"><div class=\"BuildingPrices-cell\">1...75 м²                                    </div><div class=\"BuildingPrices-cell -sqm\"><div data-currency=\"uah\" class=\"\">2 900 — 28 800 грн/м²                </div><div data-currency=\"usd\" class=\"hidden\">60 — 1 080 $/м²                </div></div></div></a>";
            const string htmlApartment2Literal = "<a href=\"/uk/жк-лебединий-київ/планування/трикімнатні\" class=\"BuildingPrices-row\" data-analytics-click=\"view_building_about|prices|goto_layout_view\"><div class=\"BuildingPrices-cell -img\"><img class=\"BuildingPrices-image lazyload\" data-src=\"//img.lunstatic.net/vector-layout/42173-1.svg\" alt=\"ЖК Лебединий: планування 3-кімнатної квартири 84.2 м2, тип 3А\"></div><div class=\"BuildingPrices-subrow\"><div class=\"BuildingPrices-cell\">3-кімнатні</div><div class=\"BuildingPrices-cell\">ід                <span data-currency=\"uah\" class=\"\">1.69 млн грн</span><span data-currency=\"usd\" class=\"hidden\">63 290 $</span></div></div><div class=\"BuildingPrices-subrow \"><div class=\"BuildingPrices-cell\">4...87 м²                                    </div><div class=\"BuildingPrices-cell -sqm\"><div data-currency=\"uah\" class=\"\">0 100 — 26 000 грн/м²                </div><div data-currency=\"usd\" class=\"hidden\">50 — 970 $/м²                </div></div></div></a>";
            const string htmlApartment3Literal = "<a href=\"/uk/жк-зарічний-київ/планування/однокімнатні\" class=\"BuildingPrices-row\" data-analytics-click=\"view_building_about|prices|goto_layout_view\"><div class=\"BuildingPrices-cell -img\"><img class=\"BuildingPrices-image lazyload\" data-src=\"//img.lunstatic.net/vector-layout/31370-0.svg\" alt=\"ЖК Зарічний: планування 1-кімнатної квартири 58.37 м2, тип 1-58.37\"></div><div class=\"BuildingPrices-subrow\"><div class=\"BuildingPrices-cell\">1-кімнатні</div><div class=\"BuildingPrices-cell\">ід                <span data-currency=\"uah\" class=\"\">1.32 млн грн</span><span data-currency=\"usd\" class=\"hidden\">49 390 $</span></div></div><div class=\"BuildingPrices-subrow \"><div class=\"BuildingPrices-cell\">3...66 м²                                    </div><div class=\"BuildingPrices-cell -sqm\"><div data-currency=\"uah\" class=\"\">2 000 — 34 650 грн/м²                </div><div data-currency=\"usd\" class=\"hidden\">20 — 1 300 $/м²                </div></div></div></a>";
            const string htmlApartment4Literal = "<a href=\"/uk/жк-зарічний-київ/планування/двокімнатні\" class=\"BuildingPrices-row\" data-analytics-click=\"view_building_about|prices|goto_layout_view\"><div class=\"BuildingPrices-cell -img\"><img class=\"BuildingPrices-image lazyload\" data-src=\"//img.lunstatic.net/vector-layout/31372-0.svg\" alt=\"ЖК Зарічний: планування 2-кімнатної квартири 92.78 м2, тип 2-92.78\"></div><div class=\"BuildingPrices-subrow\"><div class=\"BuildingPrices-cell\">2-кімнатні</div><div class=\"BuildingPrices-cell\">ід                <span data-currency=\"uah\" class=\"\">1.97 млн грн</span><span data-currency=\"usd\" class=\"hidden\">73 530 $</span></div></div><div class=\"BuildingPrices-subrow \"><div class=\"BuildingPrices-cell\">3...110 м²                                    </div><div class=\"BuildingPrices-cell -sqm\"><div data-currency=\"uah\" class=\"\">1 200 — 37 050 грн/м²                </div><div data-currency=\"usd\" class=\"hidden\">90 — 1 390 $/м²                </div></div></div></a>";
        
            return new List<HtmlNode>
            {
                HtmlNode.CreateNode(htmlApartment1Literal),
                HtmlNode.CreateNode(htmlApartment2Literal),
                HtmlNode.CreateNode(htmlApartment3Literal),
                HtmlNode.CreateNode(htmlApartment4Literal)
            };
        }

        private IEnumerable<Apartment> CreateApartments()
        {
            return new List<Apartment>
            {
                new Apartment()
                {
                    NumberOfRooms = 1,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 20,
                    DwellingSpaceMax = 30,
                    SquareMeterPriceMin = 20000,
                    SquareMeterPriceMax = 30000
                },
                new Apartment()
                {
                    NumberOfRooms = 2,
                    HasMultipleFloors = true,
                    DwellingSpaceMin = 30,
                    DwellingSpaceMax = 40,
                    SquareMeterPriceMin = 30000,
                    SquareMeterPriceMax = 40000
                },
                new Apartment()
                {
                    NumberOfRooms = 3,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 40,
                    DwellingSpaceMax = 50,
                    SquareMeterPriceMin = 40000,
                    SquareMeterPriceMax = 50000
                },
                new Apartment()
                {
                    NumberOfRooms = 4,
                    HasMultipleFloors = true,
                    DwellingSpaceMin = 50,
                    DwellingSpaceMax = 60,
                    SquareMeterPriceMin = 50000,
                    SquareMeterPriceMax = 60000
                }
            };
        }

        private IEnumerable<ApartComplex> CreateApartComplexes()
        {
            return new List<ApartComplex>
            {
                new ApartComplex(){CityName = "Київ", Name = "ЖК Лебединий", Source = "DomRia", Url = "https://domria.com/uk/жк-лебединий-київ"},
                new ApartComplex(){CityName = "Київ", Name = "ЖК Chalet", Source = "DomRia", Url = "https://domria.com/uk/жк-chalet-київ"}
            };
        }
    }
}