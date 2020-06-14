using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.Common.Services
{
    public class HtmlHandler
    {
        protected virtual async Task<HtmlNodeCollection> LoadHtmlNodes(string url, string xPath)
        {
            var web = new HtmlWeb();
            var htmlPage = await web.LoadFromWebAsync(url);
            return htmlPage.DocumentNode.SelectNodes(xPath);
        }  
        
        protected virtual string ReplaceHtmlHarmfulSymbols(string data)
        {
            var harmfulSymbols = new Dictionary<string, string>
            {
                {"&nbsp;", " "}, // non-breaking space
                {"&#x27;", "'"}
            };

            foreach (var symbol in harmfulSymbols)
                data = data.Replace(symbol.Key, symbol.Value);

            return data.Trim();
        }
        
        public string ParseText(HtmlNode htmlNode)
        {
            return ReplaceHtmlHarmfulSymbols(htmlNode.InnerText.Trim());
        }
        
        public string ParseHref(HtmlNode htmlNode)
        {
            return htmlNode.Attributes["href"].Value.Trim();
        }
        
        protected virtual string CreateUrl(string url, string hRef)
        {
            return url + hRef;
        }
        
        protected virtual string ParseHrefByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            return ParseHref(searchedHtmlNode);
        }
        
        protected virtual string ParseTextByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            return ParseText(searchedHtmlNode);
        }
        
        public string CreatePageUrl(string url, int pageNumber)
        {
            const string pageTag = "?page=";
            return url + pageTag + pageNumber;
        }
        
        protected virtual string RemoveSpaces(string data)
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
    }
}