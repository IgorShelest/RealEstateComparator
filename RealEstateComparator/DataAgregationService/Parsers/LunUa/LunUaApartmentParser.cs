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
        //private HtmlWeb _web;

        public LunUaApartmentParser()
        {
            //_web = new HtmlWeb();
        }

        //public IEnumerable<ApartComplex> ParseApartmentData()
        //{
        //    var citiesData = GetCitiesData();

        //    var apartComplexesPerAllCitiesData = GetApartComplexesPerAllCitiesData(citiesData);
        //    var apartComplexes = GetApartComplexes(apartComplexesPerAllCitiesData);
        //    SetApartments(ref apartComplexes);

        //    return apartComplexes;
        //}

        public IEnumerable<ApartComplex> ParseApartmentData()
        {
            var citiesData = GetCitiesData();
            var apartComplexes = ParseApartmentDataPerAllCitiesAsync(citiesData);
            return apartComplexes;
        }

        private IEnumerable<ApartComplex> ParseApartmentDataPerAllCitiesAsync(IEnumerable<CityData> allCitiesData)
        {
            var parseApartmentDataPerCityTasks = StartParseApartmentDataPerAllCitiesTasksAsync(allCitiesData);
            var apartComplexes = AddParseApartmentDataPerAllCitiesResultsAsync(parseApartmentDataPerCityTasks);

            return apartComplexes;
        }

        private IEnumerable<Task<IEnumerable<ApartComplex>>> StartParseApartmentDataPerAllCitiesTasksAsync(IEnumerable<CityData> allCitiesData)
        {
            var parseApartmentDataPerCityTasks = new List<Task<IEnumerable<ApartComplex>>>();
            foreach (var cityData in allCitiesData)
                parseApartmentDataPerCityTasks.Add(Task.Run(() => ParseApartmentDataPerCityAsync(cityData)));

            Task.WaitAll(parseApartmentDataPerCityTasks.ToArray());

            return parseApartmentDataPerCityTasks;
        }

        private IEnumerable<ApartComplex> AddParseApartmentDataPerAllCitiesResultsAsync(IEnumerable<Task<IEnumerable<ApartComplex>>> parseApartmentDataPerCityTasks)
        {
            var apartComplexesPerCity = new List<ApartComplex>();
            parseApartmentDataPerCityTasks.ToList().ForEach(task => apartComplexesPerCity.AddRange(task.Result));
            return apartComplexesPerCity;
        }

        private IEnumerable<ApartComplex> ParseApartmentDataPerCityAsync(CityData cityData)
        {
            try
            {
                var apartComplexesPerCityData = GetApartComplexesPerCityData(cityData);
                var apartComplexesPerCity = GetApartComplexesPerCityAsync(apartComplexesPerCityData);

                return apartComplexesPerCity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }


        private IEnumerable<CityData> GetCitiesData()
        {
            var cityHtml = LoadCityHtml();
            var cityData = CreateCityData(cityHtml);
            return cityData;
        }

        private HtmlNodeCollection LoadCityHtml()
        {
            const string cityXPath = "//*[@id='geo-control']/div[2]/div[2]/div[1]/a[*]";
            return LoadHtmlNodes(_homePageUrl, cityXPath);
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

        //private IEnumerable<ApartComplexesPerCityData> GetApartComplexesPerAllCitiesData(IEnumerable<CityData> cityData)
        //{
        //    return cityData.Select(GetApartComplexesPerCityData);
        //}

        private ApartComplexesPerCityData GetApartComplexesPerCityData(CityData cityData)
        {
            var apartComplexGroupHtml = LoadApartComplexesPerCityHtml(cityData.Url);
            return CreateApartComplexGroupData(cityData, apartComplexGroupHtml);
        }

        private HtmlNode LoadApartComplexesPerCityHtml(string url)
        {
            //*[@id="geo-control"]/div[2]/div[2]/div[1]/a[1]
            const string apartComplexGroupXPath = "/html/body/div[3]/div[2]/div[2]/a";
            return LoadHtmlNodes(url, apartComplexGroupXPath)?.First();
        }

        private ApartComplexesPerCityData CreateApartComplexGroupData(CityData cityData, HtmlNode parsedApartComplexGroupData)
        {
            return new ApartComplexesPerCityData()
            {
                CityName = cityData.Name,
                Url = _homePageUrl + ParseHref(parsedApartComplexGroupData)
            };
        }


        //private IEnumerable<ApartComplex> GetApartComplexes(IEnumerable<ApartComplexesPerCityData> apartComplexesPerCityData)
        //{
        //    var apartComplexes = new List<ApartComplex>();

        //    foreach (var data in apartComplexesPerCityData)
        //    {
        //        var apartComplexesPerCity = GetApartComplexesPerCity(data);
        //        if (apartComplexesPerCity != null)
        //            apartComplexes.AddRange(apartComplexesPerCity);

        //        break; // to delete
        //    }

        //    return apartComplexes;
        //}

        //private void SetApartments(ref IEnumerable<ApartComplex> apartComplexes)
        //{
        //    foreach (var apartComplex in apartComplexes)
        //        apartComplex.Apartments = GetApartmentsPerApartComplex(apartComplex.Url);
        //}

        //private IEnumerable<ApartComplex> GetApartComplexesPerCity(ApartComplexesPerCityData apartComplexesPerCityData)
        //{
        //    var pageNumber = 1;
        //    string currentPageUrl;
        //    var apartComplexDataPerCity = new List<ApartComplex>();

        //    do
        //    {
        //        currentPageUrl = CreatePageUrl(apartComplexesPerCityData.Url, pageNumber++);
        //        var apartComplexesPerPage = GetApartComplexesPerPage(currentPageUrl, apartComplexesPerCityData.CityName);
        //        apartComplexDataPerCity.AddRange(apartComplexesPerPage);
        //    } while (false); // (NextPageExists(currentPageUrl));

        //    return apartComplexDataPerCity;
        //}

        private IEnumerable<ApartComplex> GetApartComplexesPerCityAsync(ApartComplexesPerCityData apartComplexesPerCityData)
        {
            var getApartComplexesPerPageTasks = StartGetApartComplexesPerAllPagesTasksAsync(apartComplexesPerCityData);
            var apartComplexDataPerCity = AddGetApartComplexesPerAllPagesResultsAsync(getApartComplexesPerPageTasks);

            return apartComplexDataPerCity;
        }

        private IEnumerable<Task<IEnumerable<ApartComplex>>> StartGetApartComplexesPerAllPagesTasksAsync(ApartComplexesPerCityData apartComplexesPerCityData)
        {
            var pageNumber = 1;
            string currentPageUrl;
            var getApartComplexesPerPageTasks = new List<Task<IEnumerable<ApartComplex>>>();

            do
            {
                currentPageUrl = CreatePageUrl(apartComplexesPerCityData.Url, pageNumber++);
                getApartComplexesPerPageTasks.Add(Task.Run(() => GetApartComplexesPerPageAsync(currentPageUrl, apartComplexesPerCityData.CityName)));
            }
            while (false); // (NextPageExists(currentPageUrl));

            Task.WaitAll(getApartComplexesPerPageTasks.ToArray());

            return getApartComplexesPerPageTasks;
        }

        private IEnumerable<ApartComplex> AddGetApartComplexesPerAllPagesResultsAsync(IEnumerable<Task<IEnumerable<ApartComplex>>> getApartComplexesPerPageTasks)
        {
            var apartComplexDataPerCity = new List<ApartComplex>();
            getApartComplexesPerPageTasks.ToList().ForEach(task => apartComplexDataPerCity.AddRange(task.Result));

            return apartComplexDataPerCity;
        }

        private string CreatePageUrl(string url, int pageNumber)
        {
            const string pageTag = "?page=";
            return url + pageTag + pageNumber;
        }

        //private IEnumerable<ApartComplex> GetApartComplexesPerPage(string currentPageUrl, string cityName)
        //{
        //    var apartComplexesPerCityHtml = LoadApartComplexesPerPageHtml(currentPageUrl);
        //    var apartComplexesPerCity = CreateApartComplexesPerPage(apartComplexesPerCityHtml, cityName);
        //    return apartComplexesPerCity;
        //}

        private IEnumerable<ApartComplex> GetApartComplexesPerPageAsync(string currentPageUrl, string cityName)
        {
            try
            {
                var apartComplexesPerCityHtml = LoadApartComplexesPerPageHtml(currentPageUrl);
                var apartComplexesPerCity = CreateApartComplexesPerPage(apartComplexesPerCityHtml, cityName);

                SetApartmentsAsync(ref apartComplexesPerCity);

                return apartComplexesPerCity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private void SetApartmentsAsync(ref IEnumerable<ApartComplex> apartComplexes)
        {
            var setApartmentsTasks = apartComplexes.Select(complex => Task.Run(() => SetApartmentsPerApartComplexAsync(ref complex)));
            Task.WaitAll(setApartmentsTasks.ToArray());
        }

        private void SetApartmentsPerApartComplexAsync(ref ApartComplex apartComplex)
        {
            try
            {
                var htmlNodes = LoadHtmlApartments(apartComplex.Url);
                apartComplex.Apartments = CreateApartmentsPerApartComplex(htmlNodes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private HtmlNodeCollection LoadApartComplexesPerPageHtml(string url)
        {
            const string apartComplexXPath = "//*[@id='search-results']/div[3]/div[*]/div";
            return LoadHtmlNodes(url, apartComplexXPath);
        }

        private IEnumerable<ApartComplex> CreateApartComplexesPerPage(HtmlNodeCollection apartComplexesPerPageHtml, string cityName)
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

        private HtmlNodeCollection LoadHtmlNodes(string url, string xPath)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlPage = web.Load(url);
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

        //private IEnumerable<Apartment> GetApartmentsPerApartComplex(string url)
        //{
        //    var htmlNodes = LoadHtmlApartments(url);
        //    return CreateApartmentsPerApartComplex(htmlNodes);
        //}

        private HtmlNodeCollection LoadHtmlApartments(string url)
        {
            try
            {
                const string apartmentXPath = "//*[@id='prices']/div[4]/div[@class='BuildingPrices-table']/a[@class='BuildingPrices-row']";
                return LoadHtmlNodes(url, apartmentXPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
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

        private bool NextPageExists(string currentPageUrl)
        {
            const string pageNumberXPath = "//*[@id='search-results']/div[4]/div/button[@data-analytics-click='catalog|pagination|page_click']";
            var htmlNodes = LoadHtmlNodes(currentPageUrl, pageNumberXPath);

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
