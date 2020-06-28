using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.Common;
using DataAggregationService.Aggregators.Common.Services;
using DataAgregationService.Aggregators.LunUa;
using HtmlAgilityPack;
using Moq;
using Xunit;

namespace DataAggregationService.Tests
{
    public class ApartComplexTests
    {
        [Fact]
        public async Task GetApartComplexes()
        {
            // Arrange
            var cityData = CreateCityData();
            var apartComplexGroupHrefs = CreateApartComplexGroupHrefs();
            var apartComplexGroupUrls = CreateApartComplexGroupUrls(apartComplexGroupHrefs);
            var apartComplexGroupHtmls = CreateApartComplexGroupHtmls(apartComplexGroupHrefs);
            var apartComplexGroupData = CreateApartComplexGroupData(cityData, apartComplexGroupUrls);
            var apartComplexGroupPageUrls = CreatePageUrls(apartComplexGroupData);
            var apartComplexHtmls = CreateApartComplexHtmls();
            var apartComplexes = CreateApartComplexes(apartComplexHtmls, cityData);
            var expectedResult = CreateExpectedResult(apartComplexes).ToList();
            
            var htmlWebMoc = new Mock<HtmlWeb>();
            var htmlParserMock = MockHtmlParser(htmlWebMoc, apartComplexGroupHtmls, apartComplexGroupHrefs);
            var pageHandlerMock = MockPageHandler(
                htmlParserMock, 
                cityData, 
                apartComplexGroupHtmls, 
                apartComplexGroupHrefs, 
                apartComplexGroupUrls, 
                apartComplexGroupData,
                apartComplexGroupPageUrls,
                apartComplexHtmls,
                apartComplexes);
            var apartComplexHandler = new ApartComplexHandler(pageHandlerMock.Object, htmlParserMock.Object);

            // Act
            var actualResult = (await apartComplexHandler.GetApartComplexes(cityData)).ToList();
            
            // Assert
            Assert.Equal(expectedResult.Count(), actualResult.Count());

            for (var iter = 0; iter < expectedResult.Count(); iter++)
                Assert.True(CompareApartComplexes(expectedResult[iter], actualResult[iter]));
        }

        private bool CompareApartComplexes(ApartComplex lhs, ApartComplex rhs)
        {
            return lhs.Source == rhs.Source
                   && lhs.Name == rhs.Name
                   && lhs.CityName == rhs.CityName
                   && lhs.Url == rhs.Url;
        }

        private IEnumerable<ApartComplex> CreateExpectedResult(IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> apartComplexes)
        {
            var expectedResult = new List<ApartComplex>();
            apartComplexes.ToList().ForEach(group =>
            {
                group.ToList().ForEach(page => expectedResult.AddRange(page));
            });

            return expectedResult;
        }

        private IEnumerable<string> CreateApartComplexGroupUrls(IEnumerable<string> apartComplexGroupHrefsInput)
        {
            const string lunUaUrl = "https://lun.ua";
            var apartComplexGroupUrls = apartComplexGroupHrefsInput.Select(href => $"{lunUaUrl}{href}");
            
            return apartComplexGroupUrls;
        }

        private IEnumerable<string> CreateApartComplexGroupHrefs()
        {
            return new List<string>
            {
                "/uk/новобудови-києва",
                "/uk/новобудови-київської-області"
            };
        }

        private Mock<HtmlParser> MockHtmlParser(Mock<HtmlWeb> htmlWebMoc, IEnumerable<HtmlNode> apartComplexGroupHtml, IEnumerable<string> apartComplexGroupHref)
        {
            var htmlParserMock = new Mock<HtmlParser>(htmlWebMoc.Object);
            MockParseHref(htmlParserMock, apartComplexGroupHtml, apartComplexGroupHref);
            
            return htmlParserMock;
        }

        private void MockParseHref(Mock<HtmlParser> htmlParserMock, IEnumerable<HtmlNode> apartComplexGroupHtmlInput, IEnumerable<string> apartComplexGroupHrefInput)
        {
            var apartComplexGroupHref = apartComplexGroupHrefInput.ToList();
            var apartComplexGroupHtml = apartComplexGroupHtmlInput.ToList();

            for (var iter = 0; iter < apartComplexGroupHtml.Count(); iter++)
            {
                htmlParserMock
                    .Setup(htmlParser => htmlParser.ParseHref(apartComplexGroupHtml[iter]))
                    .Returns(apartComplexGroupHref[iter]);
            }
        }

        private Mock<PageHandler> MockPageHandler(
            Mock<HtmlParser> htmlParser, 
            IEnumerable<CityData> cityData, 
            IEnumerable<HtmlNode> apartComplexGroupHtml, 
            IEnumerable<string> apartComplexGroupHref, 
            IEnumerable<string> apartComplexGroupUrls,
            IEnumerable<ApartComplexesGroupData> apartComplexesGroupDatas,
            IEnumerable<IEnumerable<string>> apartComplexGroupPageUrls,
            IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmls,
            IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> apartComplexes)
        {
            var pageHandlerMock = new Mock<PageHandler>(htmlParser.Object);
            MockLoadApartComplexDataHtml(pageHandlerMock, cityData, apartComplexGroupHtml);
            MockCreateLunUaUrl(pageHandlerMock, apartComplexGroupHref, apartComplexGroupUrls);
            MockCreatePageUrl(pageHandlerMock, apartComplexesGroupDatas, apartComplexGroupPageUrls);
            MockLoadApartComplexesHtml(pageHandlerMock, apartComplexHtmls, apartComplexGroupPageUrls);
            MockParseApartComplexText(pageHandlerMock, apartComplexHtmls, apartComplexes);
            MockParseApartComplexHRef(pageHandlerMock, apartComplexHtmls, apartComplexes);
            MockCreateLunUaUrl(pageHandlerMock, apartComplexHtmls, apartComplexes);
            MockNextPageExists(pageHandlerMock, apartComplexGroupPageUrls);
            
            return pageHandlerMock;
        }

        private void MockNextPageExists(Mock<PageHandler> pageHandlerMock, IEnumerable<IEnumerable<string>> apartComplexGroupPageUrlsInput)
        {
            var apartComplexGroupPageUrlsPerGroup = apartComplexGroupPageUrlsInput.ToList();

            for (var groupIter = 0; groupIter < apartComplexGroupPageUrlsPerGroup.Count(); groupIter++)
            {
                var apartComplexGroupPageUrls = apartComplexGroupPageUrlsPerGroup[groupIter].ToList();
                for (var pageIter = 0; pageIter < apartComplexGroupPageUrls.Count(); pageIter++)
                {
                    pageHandlerMock
                        .Setup(pageHandler => pageHandler.NextPageExists(apartComplexGroupPageUrls[pageIter]))
                        .ReturnsAsync(pageIter != apartComplexGroupPageUrls.Count() - 1);
                }
            }
        }

        private void MockParseApartComplexText(Mock<PageHandler> pageHandlerMock, IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmlsInput, IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> apartComplexesInput)
        {
            var apartComplexHtmls = apartComplexHtmlsInput.ToList();
            var apartComplexes = apartComplexesInput.ToList();

            for (var groupIter = 0; groupIter < apartComplexHtmls.Count(); groupIter++)
            {
                var apartComplexHtmlsPerGroup = apartComplexHtmls[groupIter].ToList();
                var apartComlexesPerGroup = apartComplexes[groupIter].ToList();
                
                for (var pageIter = 0; pageIter < apartComplexHtmlsPerGroup.Count(); pageIter++)
                {
                    var apartComplexHtmlsPerPage = apartComplexHtmlsPerGroup[pageIter].ToList();
                    var apartComplexPerPage = apartComlexesPerGroup[pageIter].ToList();
                    
                    for (var apartComplexIter = 0; apartComplexIter < apartComplexHtmlsPerPage.Count(); apartComplexIter++)
                    {
                        pageHandlerMock
                            .Setup(pageHandler =>
                                pageHandler.ParseApartComplexText(apartComplexHtmlsPerPage[apartComplexIter]))
                            .Returns(apartComplexPerPage[apartComplexIter].Name);
                    }
                }
            }
        }

        private void MockParseApartComplexHRef(Mock<PageHandler> pageHandlerMock, IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmlsInput, IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> apartComplexesInput)
        {
            var apartComplexHtmls = apartComplexHtmlsInput.ToList();
            var apartComplexes = apartComplexesInput.ToList();

            for (var groupIter = 0; groupIter < apartComplexHtmls.Count(); groupIter++)
            {
                var apartComplexHtmlsPerGroup = apartComplexHtmls[groupIter].ToList();
                var apartComlexesPerGroup = apartComplexes[groupIter].ToList();
                
                for (var pageIter = 0; pageIter < apartComplexHtmlsPerGroup.Count(); pageIter++)
                {
                    var apartComplexHtmlsPerPage = apartComplexHtmlsPerGroup[pageIter].ToList();
                    var apartComplexPerPage = apartComlexesPerGroup[pageIter].ToList();
                    
                    for (var apartComplexIter = 0; apartComplexIter < apartComplexHtmlsPerPage.Count(); apartComplexIter++)
                    {
                        pageHandlerMock
                            .Setup(pageHandler =>
                                pageHandler.ParseApartComplexHRef(apartComplexHtmlsPerPage[apartComplexIter]))
                            .Returns(apartComplexPerPage[apartComplexIter].Url);
                    }
                }
            }
        }

        private void MockCreateLunUaUrl(Mock<PageHandler> pageHandlerMock, IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmlsInput, IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> apartComplexesInput)
        {
            var apartComplexes = apartComplexesInput.ToList();

            for (var groupIter = 0; groupIter < apartComplexes.Count(); groupIter++)
            {
                var apartComlexesPerGroup = apartComplexes[groupIter].ToList();
                
                for (var pageIter = 0; pageIter < apartComlexesPerGroup.Count(); pageIter++)
                {
                    var apartComplexPerPage = apartComlexesPerGroup[pageIter].ToList();
                    
                    for (var apartComplexIter = 0; apartComplexIter < apartComplexPerPage.Count(); apartComplexIter++)
                    {
                        pageHandlerMock
                            .Setup(pageHandler =>
                                pageHandler.CreateLunUaUrl(apartComplexPerPage[apartComplexIter].Url))
                            .Returns(apartComplexPerPage[apartComplexIter].Url);
                    }
                }
            }
        }

        private void MockLoadApartComplexesHtml(Mock<PageHandler> pageHandlerMock, IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmlsInput, IEnumerable<IEnumerable<string>> apartComplexGroupPageUrlsInput)
        {
            var apartComplexHtmls = apartComplexHtmlsInput.ToList();
            var apartComplexGroupPageUrls = apartComplexGroupPageUrlsInput.ToList();

            for (int groupIter = 0; groupIter < apartComplexGroupPageUrls.Count(); groupIter++)
            {
                var pageUrlsPerGroup = apartComplexGroupPageUrls[groupIter].ToList();
                var apartComplexHtmlsPerGroup = apartComplexHtmls[groupIter].ToList();
                    
                for (int pageIter = 0; pageIter < pageUrlsPerGroup.Count(); pageIter++)
                {
                    pageHandlerMock
                        .Setup(pageHandler => pageHandler.LoadApartComplexesHtml(pageUrlsPerGroup[pageIter]))
                        .ReturnsAsync(apartComplexHtmlsPerGroup[pageIter]);
                }
            }
        }

        private void MockCreatePageUrl(Mock<PageHandler> pageHandlerMock, IEnumerable<ApartComplexesGroupData> apartComplexesGroupDatasInput, IEnumerable<IEnumerable<string>> apartComplexGroupPageUrlsInput)
        {
            var apartComplexesGroupDatas = apartComplexesGroupDatasInput.ToList();
            var apartComplexGroupPageUrls = apartComplexGroupPageUrlsInput.ToList();

            for (var groupIter = 0; groupIter < apartComplexGroupPageUrls.Count(); groupIter++)
            {
                for (var pageIter = 0; pageIter < apartComplexGroupPageUrls[groupIter].Count(); pageIter++)
                {
                    pageHandlerMock
                        .Setup(pageHandler => pageHandler.CreatePageUrl(apartComplexesGroupDatas[groupIter].Url, pageIter + 1))
                        .Returns(apartComplexGroupPageUrls[groupIter].ToList()[pageIter]);     
                }
            }
        }

        private void MockCreateLunUaUrl(Mock<PageHandler> pageHandlerMock, IEnumerable<string> apartComplexGroupHrefInput, IEnumerable<string> apartComplexGroupUrlsInput)
        {
            var apartComplexGroupUrls = apartComplexGroupUrlsInput.ToList();
            var apartComplexGroupHref = apartComplexGroupHrefInput.ToList();

            for (var iter = 0; iter < apartComplexGroupHref.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.CreateLunUaUrl(apartComplexGroupHref[iter]))
                    .Returns(apartComplexGroupUrls[iter]);
            }
        }

        private IEnumerable<CityData> CreateCityData()
        {
            return new List<CityData>
            {
                new CityData() {Name = "Київ", Url = "https://lun.ua/?q=kyiv"},
                new CityData() {Name = "Київська обл.", Url = "https://lun.ua/?q=kyiv_region"}
            };
        }

        private void MockLoadApartComplexDataHtml(Mock<PageHandler> pageHandlerMock, IEnumerable<CityData> cityDataInput, IEnumerable<HtmlNode> apartComplexGroupHtmlInput)
        {
            var cityData = cityDataInput.ToList();
            var apartComplexGroupHtml = apartComplexGroupHtmlInput.ToList();

            for (var iter = 0; iter < cityData.Count(); iter++)
            {
                pageHandlerMock
                    .Setup(pageHandler => pageHandler.LoadApartComplexDataHtml(cityData[iter].Url))
                    .ReturnsAsync(apartComplexGroupHtml[iter]);
            }
        }

        private IEnumerable<HtmlNode> CreateApartComplexGroupHtmls(IEnumerable<string> apartComplexGroupHrefInput)
        {
            var apartComplexGroupHref = apartComplexGroupHrefInput.ToList();
            
            var apartComplexGroup1HtmlLiteral = $"<a href=\"{apartComplexGroupHref[0]}\" data-analytics-click=\"main|buildings_list|goto_view_building\" class=\"chips-chip -dark\">83                        <svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" height=\"24\" viewbox=\"0 0 24 24\" width=\"24\" fill=\"#bdbdbd\"><path d=\"M0 0h24v24H0V0z\" fill=\"none\"></path><path d=\"M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z\"></path></svg></a>";
            var apartComplexGroup2HtmlLiteral = $"<a href=\"{apartComplexGroupHref[1]}\" data-analytics-click=\"main|buildings_list|goto_view_building\" class=\"chips-chip -dark\">33                        <svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" height=\"24\" viewbox=\"0 0 24 24\" width=\"24\" fill=\"#bdbdbd\"><path d=\"M0 0h24v24H0V0z\" fill=\"none\"></path><path d=\"M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z\"></path></svg></a>";
            
            return new List<HtmlNode>
            {
                HtmlNode.CreateNode(apartComplexGroup1HtmlLiteral),
                HtmlNode.CreateNode(apartComplexGroup2HtmlLiteral)
            };
        }

        private IEnumerable<ApartComplexesGroupData> CreateApartComplexGroupData(IEnumerable<CityData> cityData, IEnumerable<string> apartComplexGroupUrls)
        {
            var apartComplexDataPerCities = cityData.Zip(apartComplexGroupUrls, (first, second) =>
                new ApartComplexesGroupData
                {
                    CityName = first.Name,
                    Url = second
                });

            return apartComplexDataPerCities;
        }

        private IEnumerable<IEnumerable<string>> CreatePageUrls(IEnumerable<ApartComplexesGroupData> apartComplexesGroupDatasInput)
        {
            const int numberOfPages = 2;
            const string pageTag = "?page=";
            var apartComplexesGroupDatas = apartComplexesGroupDatasInput.ToList();
            
            var pageUrls = new List<List<string>>();
            
            for (var groupIter = 0; groupIter < apartComplexesGroupDatas.Count(); groupIter++)
            {
                var pageUrlsPerGroup = new List<string>();
                
                for (var pageIter = 1; pageIter <= numberOfPages; pageIter++)
                {
                    pageUrlsPerGroup.Add(apartComplexesGroupDatas[groupIter].Url + pageTag + pageIter);
                }

                pageUrls.Add(pageUrlsPerGroup);
            }

            return pageUrls;
        }

        private IEnumerable<IEnumerable<HtmlNodeCollection>> CreateApartComplexHtmls()
        {
            const string apartComplex1Literal = "<div class=\"card\"><a href=\"/uk/жк-manhattan-одеса\" class=\"card-media\" data-analytics-click=\"catalog|buildings_list|goto_view_building\" data-impression=\"4707|Catalog Page|1\"><img id=\"card-4707-animation\" class=\"card-image\" src=\"//img.lunstatic.net/building-300x300/38884.jpg\" srcset=\"//img.lunstatic.net/building-600x600/38884.jpg 2x\" alt=\"ЖК Manhattan\"><div class=\"card-label\"><div class=\"card-label-icon\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"13\" viewbox=\"0 0 20 13\" fill=\"#9e9e9e\"><path fill-rule=\"nonzero\" d=\"M10 0C4.48 0 0 2.24 0 5c0 2.24 2.94 4.13 7 4.77V13l4-4-4-4v2.73C3.85 7.17 2 5.83 2 5c0-1.06 3.04-3 8-3s8 1.94 8 3c0 .73-1.46 1.89-4 2.53v2.05c3.53-.77 6-2.53 6-4.58 0-2.76-4.48-5-10-5z\"></path></svg></div></div><div class=\"button -icon favorite\" data-analytics-click=\"catalog|buildings_list|favorites_click\" data-favorites=\"buildings-4707\" data-animation-trigger-for=\"card-4707-animation\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewbox=\"0 0 24 24\"><defs><path d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09A5.99 5.99 0 0 0 7.5 3 5.45 5.45 0 0 0 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5a3.9 3.9 0 0 1 3.57 2.36h1.87A3.88 3.88 0 0 1 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path></defs><g fill-rule=\"evenodd\"><g><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path fill=\"#fff\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><path d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"></path></g></g></svg></div><div class=\"card-content\"><div class=\"card-location\">Київський</div><div class=\"card-title\">ЖК Manhattan</div></div></a><div class=\"card-content\"><div class=\"card-price\">ід 563 тис. грн                                                </div><div class=\"card-text\">2 будинки будується, 2 підготовчі роботи, 2 в проекті</div><div class=\"card-text \">Graf development</div><a href=\"https://lun.ua/go?placement=card_catalog&amp;building_id=4707&amp;to=https%3A%2F%2Fmht.od.ua%2F\" rel=\"nofollow\" data-analytics-click=\"catalog|buildings_list|goto_away\" target=\"_blank\" class=\"card-action \">mht.od.ua</a><div class=\"card-logo\"><a href=\"/uk/graf-development\" data-analytics-click=\"catalog|buildings_list|goto_view_developer\"><img src=\"//img.lunstatic.net/company-premium/966.svg\" alt=\"Graf development\"></a></div></div></div>";
            const string apartComplex2Literal = "<div class=\"card\"><a href=\"/uk/жк-олімпійський-одеса\" class=\"card-media\" data-analytics-click=\"catalog|buildings_list|goto_view_building\" data-impression=\"5651|Catalog Page|2\"><img id=\"card-5651-animation\" class=\"card-image\" src=\"//img.lunstatic.net/building-300x300/36384.jpg\" srcset=\"//img.lunstatic.net/building-600x600/36384.jpg 2x\" alt=\"Олімпійський\"><div class=\"card-label\"><div class=\"card-label-icon\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"13\" viewbox=\"0 0 20 13\" fill=\"#9e9e9e\"><path fill-rule=\"nonzero\" d=\"M10 0C4.48 0 0 2.24 0 5c0 2.24 2.94 4.13 7 4.77V13l4-4-4-4v2.73C3.85 7.17 2 5.83 2 5c0-1.06 3.04-3 8-3s8 1.94 8 3c0 .73-1.46 1.89-4 2.53v2.05c3.53-.77 6-2.53 6-4.58 0-2.76-4.48-5-10-5z\"></path></svg></div></div><div class=\"button -icon favorite\" data-analytics-click=\"catalog|buildings_list|favorites_click\" data-favorites=\"buildings-5651\" data-animation-trigger-for=\"card-5651-animation\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewbox=\"0 0 24 24\"><defs><path d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09A5.99 5.99 0 0 0 7.5 3 5.45 5.45 0 0 0 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5a3.9 3.9 0 0 1 3.57 2.36h1.87A3.88 3.88 0 0 1 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path></defs><g fill-rule=\"evenodd\"><g><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path fill=\"#fff\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><path d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"></path></g></g></svg></div><div class=\"card-content\"><div class=\"card-location\">Приморський</div><div class=\"card-title\">лімпійський</div></div></a><div class=\"card-content\"><div class=\"card-price\">ід 1.64 млн грн                                                </div><div class=\"card-text\">1 будинок збудовано</div><div class=\"card-text \">Стікон</div><a href=\"https://lun.ua/go?placement=card_catalog&amp;building_id=5651&amp;to=https%3A%2F%2Folimp.stikon.od.ua%2F\" rel=\"nofollow\" data-analytics-click=\"catalog|buildings_list|goto_away\" target=\"_blank\" class=\"card-action \">olimp.stikon.od.ua</a><div class=\"card-logo\"><a href=\"/uk/стікон\" data-analytics-click=\"catalog|buildings_list|goto_view_developer\"><img src=\"//img.lunstatic.net/company-premium/958.svg\" alt=\"Стікон\"></a></div></div></div>";
            const string apartComplex3Literal = "<div class=\"card\"><a href=\"/uk/жк-посейдон-одеса\" class=\"card-media\" data-analytics-click=\"catalog|buildings_list|goto_view_building\" data-impression=\"5882|Catalog Page|3\"><img id=\"card-5882-animation\" class=\"card-image\" src=\"//img.lunstatic.net/building-300x300/41658.jpg\" srcset=\"//img.lunstatic.net/building-600x600/41658.jpg 2x\" alt=\"ЖК Посейдон\"><div class=\"card-label\"><div class=\"label\" style=\"background: #ff9800\">Нова черга</div><div class=\"card-label-icon\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"13\" viewbox=\"0 0 20 13\" fill=\"#9e9e9e\"><path fill-rule=\"nonzero\" d=\"M10 0C4.48 0 0 2.24 0 5c0 2.24 2.94 4.13 7 4.77V13l4-4-4-4v2.73C3.85 7.17 2 5.83 2 5c0-1.06 3.04-3 8-3s8 1.94 8 3c0 .73-1.46 1.89-4 2.53v2.05c3.53-.77 6-2.53 6-4.58 0-2.76-4.48-5-10-5z\"></path></svg></div></div><div class=\"button -icon favorite\" data-analytics-click=\"catalog|buildings_list|favorites_click\" data-favorites=\"buildings-5882\" data-animation-trigger-for=\"card-5882-animation\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewbox=\"0 0 24 24\"><defs><path d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09A5.99 5.99 0 0 0 7.5 3 5.45 5.45 0 0 0 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5a3.9 3.9 0 0 1 3.57 2.36h1.87A3.88 3.88 0 0 1 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path></defs><g fill-rule=\"evenodd\"><g><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path fill=\"#fff\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><path d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"></path></g></g></svg></div><div class=\"card-content\"><div class=\"card-location\">Київський</div><div class=\"card-title\">ЖК Посейдон</div></div></a><div class=\"card-content\"><div class=\"card-price\">ід 655 тис. грн                                                </div><div class=\"card-text\">1 будинок будується</div><div class=\"card-text \">Гефест</div><a href=\"https://lun.ua/go?placement=card_catalog&amp;building_id=5882&amp;to=https%3A%2F%2Fgefest.ua%2Fobjects%2Fposeidon%2F\" rel=\"nofollow\" data-analytics-click=\"catalog|buildings_list|goto_away\" target=\"_blank\" class=\"card-action \">gefest.ua</a><div class=\"card-logo\"><a href=\"/uk/гефест\" data-analytics-click=\"catalog|buildings_list|goto_view_developer\"><img src=\"//img.lunstatic.net/company-premium/938.svg\" alt=\"Гефест\"></a></div></div></div>";
            const string apartComplex4Literal = "<div class=\"card\"><a href=\"/uk/жк-олексіївський-одеса\" class=\"card-media\" data-analytics-click=\"catalog|buildings_list|goto_view_building\" data-impression=\"6944|Catalog Page|4\"><img id=\"card-6944-animation\" class=\"card-image\" src=\"//img.lunstatic.net/building-300x300/39508.jpg\" srcset=\"//img.lunstatic.net/building-600x600/39508.jpg 2x\" alt=\"ЖК Олексіївський\"><div class=\"card-label\"></div><div class=\"button -icon favorite\" data-analytics-click=\"catalog|buildings_list|favorites_click\" data-favorites=\"buildings-6944\" data-animation-trigger-for=\"card-6944-animation\"><svg class=\"ico\" xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewbox=\"0 0 24 24\"><defs><path d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09A5.99 5.99 0 0 0 7.5 3 5.45 5.45 0 0 0 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5a3.9 3.9 0 0 1 3.57 2.36h1.87A3.88 3.88 0 0 1 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path></defs><g fill-rule=\"evenodd\"><g><path d=\"M0 0h24v24H0z\" fill=\"none\"></path><path fill=\"#fff\" d=\"M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z\"></path><path d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"></path></g></g></svg></div><div class=\"card-content\"><div class=\"card-location\">Малиновський</div><div class=\"card-title\">ЖК Олексіївський</div></div></a><div class=\"card-content\"><div class=\"card-price\">ід 587 тис. грн                                                </div><div class=\"card-text\">1 будинок підготовчі роботи</div><div class=\"card-text \">Materik</div><a href=\"https://lun.ua/go?placement=card_catalog&amp;building_id=6944&amp;to=https%3A%2F%2Falekseevskiy.od.ua%2F\" rel=\"nofollow\" data-analytics-click=\"catalog|buildings_list|goto_away\" target=\"_blank\" class=\"card-action \">alekseevskiy.od.ua</a><div class=\"card-logo\"><a href=\"/uk/materik\" data-analytics-click=\"catalog|buildings_list|goto_view_developer\"><img src=\"//img.lunstatic.net/company-premium/1056.svg\" alt=\"Materik\"></a></div></div></div>";

            return new List<List<HtmlNodeCollection>>
            {
                new List<HtmlNodeCollection>
                {
                    new HtmlNodeCollection(null)
                    {
                        HtmlNode.CreateNode(apartComplex1Literal)
                    },
                    new HtmlNodeCollection(null)
                    {
                        HtmlNode.CreateNode(apartComplex2Literal),  
                    }
                },
                new List<HtmlNodeCollection>
                {
                    new HtmlNodeCollection(null)
                    {
                        HtmlNode.CreateNode(apartComplex3Literal)
                    },
                    new HtmlNodeCollection(null)
                    {
                        HtmlNode.CreateNode(apartComplex4Literal)
                    }
                }
            };
        }

        private IEnumerable<IEnumerable<IEnumerable<ApartComplex>>> CreateApartComplexes(IEnumerable<IEnumerable<HtmlNodeCollection>> apartComplexHtmlsInput, IEnumerable<CityData> cityDataInput)
        {
            var apartComplexHtmls = apartComplexHtmlsInput.ToList();
            var apartComplexes = new List<List<List<ApartComplex>>>();
            const string source = "LunUa";

            for (var groupIter = 0; groupIter < apartComplexHtmls.Count(); groupIter++)
            {
                var apartComplexesPerGroup = new List<List<ApartComplex>>();
                var cityData = cityDataInput.ToList();
                var apartComplexHtmlsPerGroup = apartComplexHtmls[groupIter].ToList();
                
                for (var pageIter = 0; pageIter < apartComplexHtmlsPerGroup.Count(); pageIter++)
                {
                    var apartComplexHtmlsPerPage = apartComplexHtmlsPerGroup[pageIter].ToList();
                    var apartComplexesPerPage = new List<ApartComplex>();

                    for (var apartComplexIter = 0; apartComplexIter < apartComplexHtmlsPerPage.Count(); apartComplexIter++)
                    {
                        apartComplexesPerPage.Add(new ApartComplex
                        {
                            Source = source,
                            Name = $"ApartComplexName_{apartComplexIter}",
                            CityName = cityData[groupIter].Name,
                            Url = $"UrlFor_ApartComplexName_{apartComplexIter}_CityName_{groupIter}_Page_{pageIter}"
                        });
                    }
                    
                    apartComplexesPerGroup.Add(apartComplexesPerPage);
                }
                
                apartComplexes.Add(apartComplexesPerGroup);
            }

            return apartComplexes;
        }
    }
}