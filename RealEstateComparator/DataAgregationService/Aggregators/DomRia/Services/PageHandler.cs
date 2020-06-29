using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAggregationService.Aggregators.Common.Services;
using DataAggregationService.Aggregators.DomRia.Data;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.DomRia.Services
{
    public class PageHandler
    {
        private readonly HtmlParser _htmlParser;
        private static readonly string _homePageUrl = "https://dom.ria.com";
        
        public PageHandler(HtmlParser htmlParser)
        {
            _htmlParser = htmlParser;    
        }

        public virtual string CreateDomRiaUrl(string hRef)
        {
            return _htmlParser.CreateUrl(_homePageUrl, hRef);
        }
        
        public virtual string CreatePageUrl(string url, int pageNumber)
        {
            const string pageTag = "?page=";
            return url + pageTag + pageNumber;
        }

        public virtual string ParseApartComplexText(HtmlNode htmlNode)
        {
            const string apartComplexNameXPath = ".//div/div/h2/a";
            return _htmlParser.ParseTextByXPath(htmlNode, apartComplexNameXPath);
        }

        public virtual string ParseApartComplexHRef(HtmlNode htmlNode)
        {
            const string apartComplexHRefXPath = ".//div/div/h2/a";
            return _htmlParser.ParseHrefByXPath(htmlNode, apartComplexHRefXPath);
        }

        public virtual async Task<bool> NextPageExists(string currentPageUrl)
        {
            const string pageNumberXPath = "//*[@id='pagination']/div/div[1]/div/span[*]";
            var allPageNodes = await _htmlParser.LoadHtmlNodes(currentPageUrl, pageNumberXPath);
            if (allPageNodes == null)
                return false;
            
            const string activePageTag = "page-link active";
            var activePageNode = allPageNodes.FirstOrDefault(node => node.ChildNodes.FirstOrDefault(child => child.Attributes["class"].Value == activePageTag) != null);

            var lastPageNode = allPageNodes.Last();
            var nextPageExists = !activePageNode.Equals(lastPageNode);

            return nextPageExists;
        }
        
        public  virtual async Task<HtmlNodeCollection> LoadApartComplexDataHtml()
        {
            const string cityXPath = "//*[@id='app']/div[3]/div/div[3]/div[6]/ul[17]/li[*]/a";
            return  await _htmlParser.LoadHtmlNodes(_homePageUrl, cityXPath);
        }

        public virtual async Task<HtmlNodeCollection> LoadApartComplexesHtml(string url)
        {
            const string apartComplexXPath = "//*[@id='newbuilds']/section[*]";
            var apartComplexesForPage = await _htmlParser.LoadHtmlNodes(url, apartComplexXPath);
            return apartComplexesForPage;
        }

        public virtual async Task<HtmlNodeCollection> LoadApartmentsHtml(string url)
        {
            const string apartmentXPath = "//*[@id='pricesBlock']/section/div[3]/table/tbody/tr[*]";
            var apartmentNodes = await _htmlParser.LoadHtmlNodes(url, apartmentXPath);
            
            const int tableHeaderPosition = 0;
            apartmentNodes?.RemoveAt(tableHeaderPosition);
            return apartmentNodes;
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
        
        public virtual Tuple<int, int> ParseHtmlRoomSpace(HtmlNode apartment, ref ApartmentTransferData transferData)
        {
            const string roomSpaceXPath = ".//span/b";
            var thisApartmentSpaceText = apartment.SelectSingleNode(roomSpaceXPath).InnerText;
            
            var minSpace = Convert.ToInt16(float.Parse(thisApartmentSpaceText));
            var maxSpace = transferData.PreviousSpace == 0 ? minSpace : transferData.PreviousSpace;

            transferData.PreviousSpace = minSpace;
            
            return new Tuple<int, int>(minSpace, maxSpace);
        }

        public virtual Tuple<int, int> ParseHtmlApartPrice(HtmlNode apartment, ref ApartmentTransferData transferData)
        {
            const string priceXPath = ".//td[3]/span/span/b";
            var thisApartmentPriceText = _htmlParser.RemoveSpaces(apartment.SelectSingleNode(priceXPath).InnerText);
            
            var minPrice = int.Parse(thisApartmentPriceText);
            var maxPrice = transferData.PreviousPrice == 0 ? minPrice : transferData.PreviousPrice;

            transferData.PreviousPrice = minPrice;
            
            return new Tuple<int, int>(minPrice, maxPrice);
        }

        private string LoadHtmlNumOfRoomsOrFloors(HtmlNode apartment)
        {
            const string numOfRoomsXPath = ".//a";
            return _htmlParser.ParseTextByXPath(apartment, numOfRoomsXPath);
        }
    }
}