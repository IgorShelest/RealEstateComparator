using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataAgregationService.Models;
using DataAgregationService.ParsedData.LunUa;
using HtmlAgilityPack;

namespace DataAgregationService.Parsers
{
    class LunUaApartmentParser : IApartmentParser
    {
        private static readonly string _homePageUrl = "https://lun.ua";
        private HtmlWeb _web;

        public LunUaApartmentParser()
        {
            _web = new HtmlWeb();
        }

        public ICollection<ApartComplex> ParseApartmentData()
        {
            var cityData = GetCityData();
            var apartComplexGroupData = GetApartComplexGroupData(cityData);

            var apartComplexes = GetApartComplexes(apartComplexGroupData);
            SetApartments(ref apartComplexes);

            return apartComplexes;
        }

        private ICollection<CityData> GetCityData()
        {
            const string cityXPath = "//*[@id='geo-control']/div[2]/div[2]/div/a[@data-search='list-item']";
            var cityTextAndHrefPairs = ParseHtmlTextAndHRef(_homePageUrl, cityXPath);

            var cityData = new List<CityData>();
            foreach (var data in cityTextAndHrefPairs)
            {
                cityData.Add(new CityData
                {
                    Name = data.Item1,
                    Url = _homePageUrl + data.Item2
                });
            }

            return cityData;
        }

        private ICollection<ApartComplexGroupData> GetApartComplexGroupData(ICollection<CityData> cityData)
        {

            const string apartComplexGroupXPath = "/html/body/div[3]/div[2]/div[2]/a[@data-analytics-click='buildings_list|goto_view_building']";
            var apartComplexesGroupData = new List<ApartComplexGroupData>();

            foreach (var data in cityData)
            {
                var apartComplexGroupHRef = ParseHtmlHRef(data.Url, apartComplexGroupXPath);

                apartComplexesGroupData.Add(new ApartComplexGroupData
                {
                    CityName = data.Name,
                    Url = _homePageUrl + apartComplexGroupHRef
                });

                break; // to delete
            }

            return apartComplexesGroupData;
        }

        private ICollection<ApartComplex> GetApartComplexes(ICollection<ApartComplexGroupData> apartComplexGroupData)
        {
            var apartComplexes = new List<ApartComplex>();

            foreach (var data in apartComplexGroupData)
            {
                var apartComplexDataPerCity = GetCityApartComplexes(data);
                if (apartComplexDataPerCity != null)
                    apartComplexes.AddRange(apartComplexDataPerCity);

                break; // to delete
            }

            return apartComplexes;
        }

        private void SetApartments(ref ICollection<ApartComplex> apartComplexes)
        {
            const string apartmentXPath = "//*[@id='prices']/div[*]/div/a[@class='BuildingPrices-row']";
            foreach (var apartComplex in apartComplexes)
            {
                var apartmentsPerApartComplex = ParseHtmlApartmentsPerApartComplex(apartComplex.Url, apartmentXPath);
                apartComplex.Apartments = apartmentsPerApartComplex;
            }
        }

        private ICollection<ApartComplex> GetCityApartComplexes(ApartComplexGroupData apartComplexGroupData)
        {
            const string apartComplexHRefXPath = "//*[@id='search-results']/div[3]/div[*]/div/a[@data-analytics-click='buildings_list|goto_view_building']";
            const string apartComplexNameXPath = "//*[@id='search-results']/div[3]/div[*]/div/a/div[3]/div[1]";
            const string pageNumberXPath = "//*[@id='search-results']/div[4]/div/button[@data-analytics-click='pagination|page_click']";
            bool nextPageExists = true;
            var pageNumber = 1;

            var apartComplexDataPerCity = new List<ApartComplex>();

            do
            {
                var pageTag = "?page=" + pageNumber++;
                var currentPageUrl = apartComplexGroupData.Url + pageTag;
                var apartComplexNames = ParseHtmlTexts(currentPageUrl, apartComplexNameXPath);
                var apartComplexHRefs = ParseHtmlHRefs(currentPageUrl, apartComplexHRefXPath);

                var apartComplexData = apartComplexNames.Zip(apartComplexHRefs, (name, hRef) => new ApartComplex
                {
                    Name = name,
                    CityName = apartComplexGroupData.CityName,
                    Url = _homePageUrl + hRef
                });

                if (apartComplexData != null)
                    apartComplexDataPerCity.AddRange(apartComplexData);

                nextPageExists = ParseHtmlNextPageExists(apartComplexGroupData.Url + pageTag, pageNumberXPath);
            } while (false); // nextPageExists);

            return apartComplexDataPerCity;
        }

        private IEnumerable<Tuple<string, string>> ParseHtmlTextAndHRef(string url, string xPath)
        {
            HtmlDocument htmlPage = _web.Load(url);
            var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);
            var textAndHrefPairs = htmlNodes?.Select(node =>
                new Tuple<string, string>(ReplaceHtmlHarmfulSymbols(node.InnerText.Trim()),
                    node.Attributes["href"].Value.Trim()));

            return textAndHrefPairs;
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

        private IEnumerable<string> ParseHtmlHRefs(string url, string xPath)
        {
            HtmlDocument htmlPage = _web.Load(url);
            var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);
            var hRefs = htmlNodes?.Select(node => node.Attributes["href"].Value);
            return hRefs;
        }

        private string ParseHtmlHRef(string url, string xPath)
        {
            return ParseHtmlHRefs(url, xPath)?.First();
        }

        private IEnumerable<Apartment> ParseHtmlApartmentsPerApartComplex(string url, string xPath)
        {
            try
            {
                HtmlDocument htmlPage = _web.Load(url);
                var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);
                if (htmlNodes == null)
                    return null;

                var apartments = new List<Apartment>();

                foreach (var node in htmlNodes)
                {
                    var numOfRooms = ParseHtmlNumOfRooms(node);
                    var roomSpace = ParseHtmlRoomSpace(node);
                    var price = ParseHtmlApartPrice(node);

                    apartments.Add(new Apartment
                    {
                        NumberOfRooms = numOfRooms,
                        DwellingSpaceMin = roomSpace.Item1,
                        DwellingSpaceMax = roomSpace.Item2,
                        SquareMeterPriceMin = price.Item1,
                        SquareMeterPriceMax = price.Item2
                    });
                }

                return apartments;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
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

        private IEnumerable<string> ParseHtmlTexts(string url, string xPath)
        {
            HtmlDocument htmlPage = _web.Load(url);
            var textNodes = htmlPage.DocumentNode.SelectNodes(xPath);
            var texts = textNodes?.Select(node => ReplaceHtmlHarmfulSymbols(node.InnerText.Trim()));

            return texts;
        }

        private bool ParseHtmlNextPageExists(string url, string xPath)
        {
            HtmlDocument htmlPage = _web.Load(url);
            var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);

            const string activePageTag = "-active";
            var activePageNode = htmlNodes.FirstOrDefault(node => node.Attributes["class"].Value == activePageTag);
            var lastPageNode = htmlNodes.Last();
            bool nextPageExists = !activePageNode.Equals(lastPageNode);

            return nextPageExists;
        }
    }
}
