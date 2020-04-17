using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAgregationService.Models;
using DataAgregationService.ParsedData.LunUa;
using HtmlAgilityPack;

namespace DataAgregationService.Parsers
{
    class LunUaApartmentParser : IApartmentParser
    {
        private static readonly string _homePageUrl = "https://lun.ua";

        public async Task<IEnumerable<ApartComplex>[]> GetApartmentData()
        {
            var citiesData = await GetCityData();
            var apartComplexes = await GetApartmentsForAllCities(citiesData);
            return apartComplexes;
        }

        private async Task<IEnumerable<ApartComplex>[]> GetApartmentsForAllCities(IEnumerable<CityData> allCitiesData)
        {
            var parseApartmentDataPerCityTasks = allCitiesData.Select(GetApartmentsForOneCity);
            var parseApartmentDataPerCityTasksAll = await Task.WhenAll(parseApartmentDataPerCityTasks);
            return parseApartmentDataPerCityTasksAll;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartmentsForOneCity(CityData cityData)
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


        private async Task<IEnumerable<CityData>> GetCityData()
        {
            var cityHtml = await LoadCityHtmlAsync();
            var cityData = CreateCityData(cityHtml);
            return cityData;
        }

        private async Task<HtmlNodeCollection> LoadCityHtmlAsync()
        {
            const string cityXPath = "//*[@id='geo-control']/div[2]/div[2]/div[1]/a[*]";
            return await LoadHtmlNodesAsync(_homePageUrl, cityXPath);
        }

        private IEnumerable<CityData> CreateCityData(HtmlNodeCollection cityHtml)
        {
            var cityData = cityHtml?.Select(node =>
                new CityData()
                {
                    Name = ParseText(node),
                    Url = _homePageUrl + ParseHref(node)
                }
            );

            return cityData;
        }

        private async Task<ApartComplexesPerCityData> GetApartComplexData(CityData cityData)
        {
            var apartComplexGroupHtml = await LoadApartComplexDataHtml(cityData.Url);
            return CreateApartComplexData(cityData, apartComplexGroupHtml);
        }

        private async Task<HtmlNode> LoadApartComplexDataHtml(string url)
        {
            const string apartComplexGroupXPath = "/html/body/div[3]/div[2]/div[2]/a";
            var apartComplexes = await LoadHtmlNodesAsync(url, apartComplexGroupXPath);

            return apartComplexes.First();
        }

        private ApartComplexesPerCityData CreateApartComplexData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesPerCityData()
            {
                CityName = cityData.Name,
                Url = _homePageUrl + ParseHref(parsedApartComplexGroupData)
            };
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesPerCityData apartComplexesPerCityData)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var getApartComplexesPerPageTasks = new List<IEnumerable<ApartComplex>>();

            do
            {
                currentPageUrl = CreatePageUrl(apartComplexesPerCityData.Url, pageNumber++);
                getApartComplexesPerPageTasks.Add(await GetApartComplexesForPage(currentPageUrl, apartComplexesPerCityData.CityName));
            }
            while (false); // (NextPageExists(currentPageUrl));

            return default;
        }

        private string CreatePageUrl(string url, int pageNumber)
        {
            const string pageTag = "?page=";
            return url + pageTag + pageNumber;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForPage(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesHtmlForPage = await LoadApartComplexesHtmlForPage(currentPageUrl);
                var apartComplexesForPage = CreateApartComplexesForPage(apartComplexesHtmlForPage, cityName);

                foreach (var complex in apartComplexesForPage)
                    complex.Apartments = await GetApartmentsForApartComplex(complex.Url);

                return apartComplexesForPage;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<IEnumerable<Apartment>> GetApartmentsForApartComplex(string url)
        {
            try
            {
                var htmlNodes = await LoadApartmentsHtml(url);
                var apartments = CreateApartmentsPerApartComplex(htmlNodes);
                return apartments;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<HtmlNodeCollection> LoadApartComplexesHtmlForPage(string url)
        {
            const string apartComplexXPath = "//*[@id='search-results']/div[3]/div[*]/div";
            var apartComplexesForPage = await LoadHtmlNodesAsync(url, apartComplexXPath);
            return apartComplexesForPage;
        }

        private IEnumerable<ApartComplex> CreateApartComplexesForPage(HtmlNodeCollection apartComplexesPerPageHtml, string cityName)
        {
            return apartComplexesPerPageHtml.Select(complex => CreateApartComplex(complex, cityName));
        }


        private ApartComplex CreateApartComplex(HtmlNode complex, string cityName)
        {
            const string apartComplexHRefXPath = ".//a";
            const string apartComplexNameXPath = ".//a/div[3]/div[@class='card-title']";

            return new ApartComplex()
            {
                Name = ParseTextByXPath(complex, apartComplexNameXPath),
                CityName = cityName,
                Url = _homePageUrl + ParseHRefByXPath(complex, apartComplexHRefXPath)
            };
        }

        private string ParseHRefByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            return ParseHref(searchedHtmlNode);
        }

        private string ParseTextByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            return ParseText(searchedHtmlNode);
        }

        private async Task<HtmlNodeCollection> LoadHtmlNodesAsync(string url, string xPath)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlPage = await web.LoadFromWebAsync(url);
            return htmlPage.DocumentNode.SelectNodes(xPath);
        }

        private static string ReplaceHtmlHarmfulSymbols(string data)
        {
            IEnumerable<string> harmfulSymbols = new List<string>
            {
                "&nbsp;", // non-breaking space
            };

            foreach (var symbol in harmfulSymbols)
                data = data.Replace(symbol, " ");

            return data.Trim();
        }

        private static string RemoveSpaces(string data)
        {
            IEnumerable<string> spaces = new List<string>
            {
                " ", // non-breaking space
                " " // space
            };

            foreach (var symbol in spaces)
                data = data.Replace(symbol, "");

            return data.Trim();
        }

        private async Task<HtmlNodeCollection> LoadApartmentsHtml(string url)
        {
            try
            {
                const string apartmentXPath = "//*[@id='prices']/div[4]/div[@class='BuildingPrices-table']/a[@class='BuildingPrices-row']";
                return await LoadHtmlNodesAsync(url, apartmentXPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private IEnumerable<Apartment> CreateApartmentsPerApartComplex(HtmlNodeCollection htmlNodes)
        {
            var apartments = htmlNodes?.Select(CreateApartment);
            return apartments;
        }

        private Apartment CreateApartment(HtmlNode node)
        {
            var numOfRooms = ParseHtmlNumOfRooms(node);
            var roomSpace = ParseHtmlRoomSpace(node);
            var price = ParseHtmlApartPrice(node);

            return new Apartment
            {
                NumberOfRooms = numOfRooms,
                DwellingSpaceMin = roomSpace.Item1,
                DwellingSpaceMax = roomSpace.Item2,
                SquareMeterPriceMin = price.Item1,
                SquareMeterPriceMax = price.Item2
            };
        }

        private string ParseHtmlNumOfRooms(HtmlNode apartment)
        {
            const string numOfRoomsXPath = "/div[2]/div[1]";
            IEnumerable<string> numOfRoomsPatterns = new List<string>
            {
                @"^(?<num>\d+)",
                @"^(?<num>[А-ЯІ][а-яі]+)"
            };

            var numOfRoomsText = RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + numOfRoomsXPath).InnerText);

            foreach (var pattern in numOfRoomsPatterns)
            {
                Regex numOfRoomsPattern = new Regex(pattern);
                Match match = numOfRoomsPattern.Match(HtmlEntity.DeEntitize(numOfRoomsText));

                if (match.Success)
                {
                    string numOfRooms = match.Groups["num"].Value;
                    return numOfRooms;
                }
            }

            return default;
        }

        private Tuple<int, int> ParseHtmlRoomSpace(HtmlNode apartment)
        {
            // Preset data
            const string minTag = "min";
            const string maxTag = "max";
            const string roomSpaceXPath = "/div[3]/div[1]";
            IEnumerable<string> roomSpacePatterns = new List<string>
            {
                String.Format(@"(?<{0}>\d+)\.\.\.(?<{1}>\d+)м²", minTag, maxTag),
                String.Format(@"(?<{0}>\d+)м²", minTag)
            };

            var roomSpaceText = RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + roomSpaceXPath).InnerText);

            foreach (var pattern in roomSpacePatterns)
            {
                Regex roomSpaceRegEx = new Regex(pattern);
                Match match = roomSpaceRegEx.Match(HtmlEntity.DeEntitize(roomSpaceText));

                if (match.Success)
                {
                    int roomSpaceMin = match.Groups[minTag].Success
                        ? int.Parse(match.Groups[minTag].Value)
                        : default(int);
                    int roomSpaceMax = match.Groups[maxTag].Success
                        ? int.Parse(match.Groups[maxTag].Value)
                        : roomSpaceMin;
                    Tuple<int, int> result = new Tuple<int, int>(roomSpaceMin, roomSpaceMax);
                    return result;
                }
            }

            return default;
        }

        private Tuple<int, int> ParseHtmlApartPrice(HtmlNode apartment)
        {
            // Preset data
            const string minTag = "min";
            const string maxTag = "max";
            const string priceXPath = "/div[3]/div[2]";
            IEnumerable<string> pricePatterns = new List<string>
            {
                String.Format(@"(?<min>\d+)(-|—)(?<max>\d+)грн\/м²", minTag, maxTag),
                String.Format(@"(?<min>\d+)грн\/м²", minTag)
            };

            var apartPriceText =
                RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + priceXPath).InnerText.Trim());

            foreach (var pattern in pricePatterns)
            {
                Regex priceRegEx = new Regex(pattern);
                Match match = priceRegEx.Match(HtmlEntity.DeEntitize(apartPriceText));

                if (match.Success)
                {
                    int priceMin = match.Groups[minTag].Success ? int.Parse(match.Groups[minTag].Value) : default(int);
                    int priceMax = match.Groups[maxTag].Success ? int.Parse(match.Groups[maxTag].Value) : priceMin;
                    Tuple<int, int> result = new Tuple<int, int>(priceMin, priceMax);
                    return result;
                }
            }

            return default;
        }

        private async Task<bool> NextPageExists(string currentPageUrl)
        {
            const string pageNumberXPath = "//*[@id='search-results']/div[4]/div/button[@data-analytics-click='catalog|pagination|page_click']";
            var htmlNodes = await LoadHtmlNodesAsync(currentPageUrl, pageNumberXPath);

            const string activePageTag = "-active";
            var activePageNode = htmlNodes.FirstOrDefault(node => node.Attributes["class"].Value == activePageTag);
            var lastPageNode = htmlNodes.Last();
            bool nextPageExists = !activePageNode.Equals(lastPageNode);

            return nextPageExists;
        }

        private string ParseText(HtmlNode htmlNode)
        {
            return ReplaceHtmlHarmfulSymbols(htmlNode.InnerText.Trim());
        }

        private string ParseHref(HtmlNode htmlNode)
        {
            return htmlNode.Attributes["href"].Value.Trim();
        }
    }
}
