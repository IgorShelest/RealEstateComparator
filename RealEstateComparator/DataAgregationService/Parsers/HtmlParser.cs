using DataAgregationService.DataSources;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using DataAgregationService.Models;

namespace DataAgregationService.Parsers
{
    class HtmlParser
    {
        private HtmlWeb _web;

        static private IEnumerable<string> _spaces = new List<string>
        {
            " ", // non-breaking space
            " "  // space
        };

        static private IEnumerable<string> _harmfulSymbols = new List<string>
        {
            "&nbsp;", // non-breaking space
        };

        static private string removeSpaces(string data)
        {
            foreach (var symbol in _spaces)
                data = data.Replace(symbol, "");

            return data.Trim();
        }

        static private string replaceHarmfulSymbols(string data)
        {
            foreach (var symbol in _harmfulSymbols)
                data = data.Replace(symbol, " ");

            return data.Trim();
        }

        public HtmlParser()
        {
            _web = new HtmlWeb();
        }

        public string ParseHtmlText(string url, string xPath) 
        {
            try
            {                
                HtmlDocument htmlPage = _web.Load(url);
                var htmlNodes = replaceHarmfulSymbols(htmlPage.DocumentNode.SelectNodes(xPath).Select(node => node.InnerText).First());
                return htmlNodes;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public IEnumerable<string> ParseHtmlTexts(string url, string xPath)
        {
            try
            {
                HtmlDocument htmlPage = _web.Load(url);
                var texts = htmlPage.DocumentNode.SelectNodes(xPath).Select(node => replaceHarmfulSymbols(node.InnerText.Trim()));

                return texts;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public IEnumerable<string> ParseHtmlHRefs(string url, string xPath)
        {
            try
            {
                HtmlDocument htmlPage = _web.Load(url);
                var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath).Select(node => node.Attributes["href"].Value);
                return htmlNodes;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public IEnumerable<Tuple<string, string>> ParseHtmlTextAndHRef(string url, string xPath)
        {
            try
            {
                HtmlDocument htmlPage = _web.Load(url);
                var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath).Select(node => new Tuple<string,string>(replaceHarmfulSymbols(node.InnerText.Trim()), node.Attributes["href"].Value.Trim()));
                return htmlNodes;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public string ParseHtmlHRef(string url, string xPath)
        {
            try
            {
                return ParseHtmlHRefs(url, xPath).First();
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }

        public bool ParseHtmlNextPageExists(string url, string xPath)
        {
            try
            {
                HtmlDocument htmlPage = _web.Load(url);
                var htmlNodes = htmlPage.DocumentNode.SelectNodes(xPath);

                const string activePageTag = "-active";
                var activePageNode = htmlNodes.FirstOrDefault(node => node.Attributes["class"].Value == activePageTag);
                var lastPageNode = htmlNodes.Last();
                bool nextPageExists = !activePageNode.Equals(lastPageNode);

                return nextPageExists;
            }
            catch (Exception ex)
            {
                // Log
            }

            return false;
        }

        public string ParseHtmlNumOfRooms(HtmlNode apartment)
        {
            try
            {
                const string numOfRoomsXPath = "/div[2]/div[1]";

                IEnumerable<string> numOfRoomsPatterns = new List<string>
                {
                    String.Format(@"^(?<num>\d+)"),
                    String.Format(@"^(?<num>[А-ЯІ][а-яі]+)")
                };

                var numOfRoomsText = removeSpaces(apartment.SelectSingleNode(apartment.XPath + numOfRoomsXPath).InnerText);

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
            }
            catch (Exception ex)
            {
                // Log
            }

            return default;
        }

        public Tuple<int, int> ParseHtmlRoomSpace(HtmlNode apartment)
        {
            try
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

                var roomSpaceText = removeSpaces(apartment.SelectSingleNode(apartment.XPath + roomSpaceXPath).InnerText);

                foreach (var pattern in roomSpacePatterns)
                {
                    Regex roomSpaceRegEx = new Regex(pattern);
                    Match match = roomSpaceRegEx.Match(HtmlEntity.DeEntitize(roomSpaceText));

                    if (match.Success)
                    {
                        int roomSpaceMin = match.Groups[minTag].Success ? int.Parse(match.Groups[minTag].Value) : default(int);
                        int roomSpaceMax = match.Groups[maxTag].Success ? int.Parse(match.Groups[maxTag].Value) : roomSpaceMin;
                        Tuple<int, int> result = new Tuple<int, int>(roomSpaceMin, roomSpaceMax);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log
            }

            return default;
        }

        public Tuple<int, int> ParseHtmlApartPrice(HtmlNode apartment)
        {
            try
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

                var apartPriceText = removeSpaces(apartment.SelectSingleNode(apartment.XPath + priceXPath).InnerText.Trim());

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
            }
            catch (Exception ex)
            {
                // Log
            }

            return default;
        }

        public IEnumerable<Apartment> ParseHtmlApartmentsPerApartComplex(string url, string xPath)
        {
            try
            {
                //IEnumerable<string> apartmentXPathes = new List<string>
                //{

                //};

                //Console.WriteLine(url + " " + xPath);

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
                    //Console.WriteLine("{0} {1} {2}", numOfRooms, roomSpace, price);
                }

                return apartments;
            }
            catch (Exception ex)
            {
                // Log
            }

            return null;
        }
    }
}
