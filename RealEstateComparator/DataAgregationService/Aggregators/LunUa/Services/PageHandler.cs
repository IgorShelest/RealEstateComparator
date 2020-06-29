using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common.Services;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.LunUa.Services
{
    public class PageHandler
    {
        private readonly HtmlParser _htmlParser;
        private static readonly string _homePageUrl = "https://lun.ua";

        public PageHandler(HtmlParser htmlParser)
        {
            _htmlParser = htmlParser;
        }
        
        public virtual string CreateLunUaUrl(string hRef)
        {
            return _htmlParser.CreateUrl(_homePageUrl, hRef);
        }
        
        public virtual string CreatePageUrl(string url, int pageNumber)
        {
            const string pageTag = "?page=";
            return url + pageTag + pageNumber;
        }
        
        public virtual async Task<HtmlNodeCollection> LoadCityHtml()
        {
            const string cityXPath = "//*[@id='geo-control']/div[3]/div[2]/div/div[4]/a[*]";
            return await _htmlParser.LoadHtmlNodes(_homePageUrl, cityXPath);
        }

        public virtual string ParseApartComplexText(HtmlNode htmlNode)
        {
            const string apartComplexNameXPath = ".//a/div[3]/div[@class='card-title']";
            return _htmlParser.ParseTextByXPath(htmlNode, apartComplexNameXPath);
        }

        public virtual string ParseApartComplexHRef(HtmlNode htmlNode)
        {
            const string apartComplexHRefXPath = ".//a";
            return _htmlParser.ParseHrefByXPath(htmlNode, apartComplexHRefXPath);
        }

        public virtual async Task<bool> NextPageExists(string currentPageUrl)
        {
            const string pageNumberXPath = "//*[@id='search-results']/div[4]/div/button[@data-analytics-click='catalog|pagination|page_click']";
            var htmlNodes = await _htmlParser.LoadHtmlNodes(currentPageUrl, pageNumberXPath);
            if (htmlNodes == null)
                return false;

            const string activePageTag = "-active";
            var activePageNode = htmlNodes.FirstOrDefault(node => node.Attributes["class"].Value == activePageTag);
            var lastPageNode = htmlNodes.Last();
            bool nextPageExists = !activePageNode.Equals(lastPageNode);

            return nextPageExists;
        }

        public virtual async Task<HtmlNode> LoadApartComplexDataHtml(string url)
        {
            const string apartComplexGroupXPath = "/html/body/div[3]/div[2]/div[2]/a[1]";
            var apartComplexes = await _htmlParser.LoadHtmlNodes(url, apartComplexGroupXPath);

            return apartComplexes.First();
        }

        public virtual async Task<HtmlNodeCollection> LoadApartComplexesHtml(string url)
        {
            const string apartComplexXPath = "//*[@id='search-results']/div[3]/div[*]/div";
            var apartComplexesForPage = await _htmlParser.LoadHtmlNodes(url, apartComplexXPath);
            return apartComplexesForPage;
        }

        public virtual async Task<HtmlNodeCollection> LoadApartmentsHtml(string url)
        {
            try
            {
                const string apartmentXPath = "//*[@id='prices']/div[4]/div[@class='BuildingPrices-table']/a[@class='BuildingPrices-row']";
                return await _htmlParser.LoadHtmlNodes(url, apartmentXPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public virtual int ParseHtmlNumOfRooms(HtmlNode apartment)
        {
            const string numOfRoomsPattern = @"^(?<num>\d+)";
            var numOfRoomsRegex = new Regex(numOfRoomsPattern);
            
            var numOfRoomsText = LoadHtmlNumOfRoomsOrFloors(apartment);
            var match = numOfRoomsRegex.Match(HtmlEntity.DeEntitize(numOfRoomsText));
            
            return match.Success ? int.Parse(match.Groups["num"].Value) : default;
        }
        
        public virtual bool HasMultipleFloors(HtmlNode apartment)
        {
            const string numOfFloorsPattern = @"^(?<num>[А-ЯІ][а-яі]+)";
            var numOfRoomsPattern = new Regex(numOfFloorsPattern);
            
            var numOfFloorsText = LoadHtmlNumOfRoomsOrFloors(apartment);
            var match = numOfRoomsPattern.Match(HtmlEntity.DeEntitize(numOfFloorsText));
        
            return match.Success;
        }
        
        public virtual Tuple<int, int> ParseHtmlRoomSpace(HtmlNode apartment)
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

            var roomSpaceText = _htmlParser.RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + roomSpaceXPath).InnerText);

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

        public virtual Tuple<int, int> ParseHtmlApartPrice(HtmlNode apartment)
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
                _htmlParser.RemoveSpaces(apartment.SelectSingleNode(apartment.XPath + priceXPath).InnerText.Trim());

            foreach (var pattern in pricePatterns)
            {
                var priceRegEx = new Regex(pattern);
                var match = priceRegEx.Match(HtmlEntity.DeEntitize(apartPriceText));

                if (!match.Success) 
                    continue;
                
                var priceMin = match.Groups[minTag].Success ? int.Parse(match.Groups[minTag].Value) : default(int);
                var priceMax = match.Groups[maxTag].Success ? int.Parse(match.Groups[maxTag].Value) : priceMin;
                var result = new Tuple<int, int>(priceMin, priceMax);
                return result;
            }

            return default;
        }

        private string LoadHtmlNumOfRoomsOrFloors(HtmlNode apartment)
        {
            const string numOfRoomsXPath = "/div[2]/div[1]";
            return _htmlParser.ParseTextByXPath(apartment, apartment.XPath + numOfRoomsXPath);
        }
    }
}