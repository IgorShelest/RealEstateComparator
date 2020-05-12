using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApplicationContexts.Models;
using DataAggregationService.Interfaces;
using DataAggregationService.ParsedData.LunUa;
using HtmlAgilityPack;

namespace DataAggregationService.Parsers.LunUa
{
    class LunUaApartmentParser : IApartmentParser
    {
        private static readonly string _homePageUrl = "https://lun.ua";

        public async Task<IEnumerable<ApartComplex>> GetApartmentData()
        {
            var citiesData = await GetCityData();
            var apartComplexes = await GetApartmentsForAllCities(citiesData);
            return apartComplexes;
        }

        private async Task<IEnumerable<ApartComplex>> GetApartmentsForAllCities(IEnumerable<CityData> allCitiesData)
        {
            var apartComplexesForAllCitiesTasks = allCitiesData.Select(GetApartmentsForOneCity);
            var apartComplexesForAllCities = await Task.WhenAll(apartComplexesForAllCitiesTasks);

            var combinedResults = new List<ApartComplex>();
            apartComplexesForAllCities.ToList().ForEach(apartments => combinedResults.AddRange(apartments));
            
            return combinedResults;
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
            var cityHtml = await LoadCityHtml();
            var cityData = CreateCityData(cityHtml);
            return cityData;
        }

        private async Task<HtmlNodeCollection> LoadCityHtml()
        {
            const string cityXPath = "//*[@id='geo-control']/div[3]/div[2]/div/div[4]/a[*]";
            return await LoadHtmlNodes(_homePageUrl, cityXPath);
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

        private async Task<ApartComplexesDataPerCity> GetApartComplexData(CityData cityData)
        {
            var apartComplexGroupHtml = await LoadApartComplexDataHtml(cityData.Url);
            return CreateApartComplexData(cityData, apartComplexGroupHtml);
        }

        private async Task<HtmlNode> LoadApartComplexDataHtml(string url)
        {
            const string apartComplexGroupXPath = "/html/body/div[3]/div[2]/div[2]/a";
            var apartComplexes = await LoadHtmlNodes(url, apartComplexGroupXPath);

            return apartComplexes.First();
        }

        private ApartComplexesDataPerCity CreateApartComplexData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesDataPerCity
            {
                CityName = cityData.Name,
                Url = _homePageUrl + ParseHref(parsedApartComplexGroupData)
            };
        }

        private async Task<IEnumerable<ApartComplex>> GetApartComplexesForAllPages(ApartComplexesDataPerCity apartComplexesDataPerCity)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var apartComplexesPerPage = new List<ApartComplex>();

            do
            {
                currentPageUrl = CreatePageUrl(apartComplexesDataPerCity.Url, pageNumber++);
                apartComplexesPerPage.AddRange(await GetApartComplexesForPage(currentPageUrl, apartComplexesDataPerCity.CityName));
            }
            while (false); // (NextPageExists(currentPageUrl));

            return apartComplexesPerPage;
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
                var apartComplexesHtml = await LoadApartComplexesHtml(currentPageUrl);
                var apartComplexes = CreateApartComplexes(apartComplexesHtml, cityName).ToList();

                foreach (var complex in apartComplexes)
                    complex.Apartments = await GetApartmentsForApartComplex(complex.Url);

                return apartComplexes;
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

        private async Task<HtmlNodeCollection> LoadApartComplexesHtml(string url)
        {
            const string apartComplexXPath = "//*[@id='search-results']/div[3]/div[*]/div";
            var apartComplexesForPage = await LoadHtmlNodes(url, apartComplexXPath);
            return apartComplexesForPage;
        }

        private IEnumerable<ApartComplex> CreateApartComplexes(HtmlNodeCollection apartComplexesPerPageHtml, string cityName)
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

        private async Task<HtmlNodeCollection> LoadHtmlNodes(string url, string xPath)
        {
            var web = new HtmlWeb();
            var htmlPage = await web.LoadFromWebAsync(url);
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
                return await LoadHtmlNodes(url, apartmentXPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private IEnumerable<Apartment> CreateApartmentsPerApartComplex(HtmlNodeCollection htmlNodes)
        {
            var apartments = htmlNodes?.Select(CreateApartment).ToList();
            return apartments;
        }

        private Apartment CreateApartment(HtmlNode node)
        {
            var numOfRooms = ParseHtmlNumOfRooms(node);
            var hasMultipleFloors = HasMultipleFloors(node);
            var roomSpace = ParseHtmlRoomSpace(node);
            var price = ParseHtmlApartPrice(node);

            return new Apartment
            {
                NumberOfRooms = numOfRooms,
                HasMultipleFloors = hasMultipleFloors,
                DwellingSpaceMin = roomSpace.Item1,
                DwellingSpaceMax = roomSpace.Item2,
                SquareMeterPriceMin = price.Item1,
                SquareMeterPriceMax = price.Item2
            };
        }

        private string LoadHtmlNumOfRoomsOrFloors(HtmlNode apartment)
        {
            const string numOfRoomsXPath = "/div[2]/div[1]";
            return RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + numOfRoomsXPath).InnerText);
        }

        private int ParseHtmlNumOfRooms(HtmlNode apartment)
        {
            const string numOfRoomsPattern = @"^(?<num>\d+)";
            var numOfRoomsRegex = new Regex(numOfRoomsPattern);
            
            var numOfRoomsText = LoadHtmlNumOfRoomsOrFloors(apartment);
            var match = numOfRoomsRegex.Match(HtmlEntity.DeEntitize(numOfRoomsText));
            
            return match.Success ? int.Parse(match.Groups["num"].Value) : default;
        }

        private bool HasMultipleFloors(HtmlNode apartment)
        {
            const string numOfFloorsPattern = @"^(?<num>[А-ЯІ][а-яі]+)";
            var numOfRoomsPattern = new Regex(numOfFloorsPattern);
            
            var numOfFloorsText = LoadHtmlNumOfRoomsOrFloors(apartment);
            var match = numOfRoomsPattern.Match(HtmlEntity.DeEntitize(numOfFloorsText));

            return match.Success;
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
            var htmlNodes = await LoadHtmlNodes(currentPageUrl, pageNumberXPath);

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
