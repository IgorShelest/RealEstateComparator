using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DataAggregationService.Aggregators.Common.Services
{
    public class HtmlParser
    {
        private readonly HtmlWeb _htmlWeb;
        
        public HtmlParser(HtmlWeb htmlWeb)
        {
            _htmlWeb = htmlWeb;
        }
        
        public virtual async Task<HtmlNodeCollection> LoadHtmlNodes(string url, string xPath)
        {
            var htmlPage = await _htmlWeb.LoadFromWebAsync(url);
            var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);
            
            return htmlNodes;
        }  
        
        public virtual string ParseText(HtmlNode htmlNode)
        {
            return ReplaceHtmlHarmfulSymbols(htmlNode.InnerText.Trim());
        }
        
        public virtual string ParseHref(HtmlNode htmlNode)
        {
            return htmlNode.Attributes["href"].Value.Trim();
        }

        public virtual string CreateUrl(string url, string hRef)
        {
            return url + hRef;
        }

        public virtual string ParseHrefByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            var hRef = ParseHref(searchedHtmlNode);

            return hRef;
        }

        public virtual string ParseTextByXPath(HtmlNode htmlNode, string xPath)
        {
            var searchedHtmlNode = htmlNode.SelectSingleNode(xPath);
            var text = ParseText(searchedHtmlNode);

            return text;
        }

        public virtual string RemoveSpaces(string data)
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
        
        private string ReplaceHtmlHarmfulSymbols(string data)
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
    }
}