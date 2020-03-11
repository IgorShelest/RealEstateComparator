using DataAgregationService.Models;
using DataAgregationService.ParsedData.LunUa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAgregationService.Parsers
{
    class LunUaParser : HtmlParser, IParser
    {
        private readonly string _homePageUrl = "https://lun.ua";

        private readonly string _cityHRef = "/html/body/div[3]/div[2]/div[1]/a[@data-analytics-click='geo_list|goto_catalog']";

        private readonly string _apartComplexGroupHRef = "/html/body/div[3]/div[2]/div[3]/a[@data-analytics-click='buildings_list|goto_view_building']";

        private readonly string _apartComplexHRef = "//*[@id='search-results']/div[3]/div[*]/div/a[@data-analytics-click='buildings_list|goto_view_building']";

        private readonly string _apartComplexName = "//*[@id='search-results']/div[3]/div[*]/div/a/div[3]/div[1]";

        private readonly string _pageNumbers = "//*[@id='search-results']/div[4]/div/button[@data-analytics-click='pagination|page_click']";

        private readonly string _apartment = "//*[@id='prices']/div[*]/div/a[@class='BuildingPrices-row']";

        private ICollection<CityData> GetCityData()
        {
            var cityTextAndHrefPairs = ParseHtmlTextAndHRef(_homePageUrl, _cityHRef);

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
            var apartComplexesGroupData = new List<ApartComplexGroupData>();
         
            foreach (var data in cityData)
            {
                var apartComplexGroupHRef = ParseHtmlHRef(data.Url, _apartComplexGroupHRef);

                apartComplexesGroupData.Add(new ApartComplexGroupData
                { 
                    CityName = data.Name,
                    Url = _homePageUrl + apartComplexGroupHRef
                });
            }

            return apartComplexesGroupData;
        }

        private ICollection<ApartComplex> GetApartComplexes(ICollection<ApartComplexGroupData> apartComplexGroupData)
        {
            var apartComplexes = new List<ApartComplex>();

            foreach (var data in apartComplexGroupData)
            {
                var apartComplexDataPerCity = GetCityApartComplexes(data);
                if(apartComplexDataPerCity != null)
                    apartComplexes.AddRange(apartComplexDataPerCity);

                break; // to delete
            }

            return apartComplexes;
        }

        private void SetApartments(ref ICollection<ApartComplex> apartComplexes)
        {
            foreach (var apartComplex in apartComplexes)
            {
                var apartmentsPerApartComplex = ParseHtmlApartmentsPerApartComplex(apartComplex.Url, _apartment);

                if (apartmentsPerApartComplex != null)
                    apartComplex.Apartments = apartmentsPerApartComplex;
            }
        }
        
        private ICollection<ApartComplex> GetCityApartComplexes(ApartComplexGroupData apartComplexGroupData)
        {
            bool nextPageExists = true;
            var pageNumber = 1;

            var apartComplexDataPerCity = new List<ApartComplex>();

            do
            {
                var pageTag = "?page=" + pageNumber++;
                var currentPageUrl = apartComplexGroupData.Url + pageTag;
                var apartComplexNames = ParseHtmlTexts(currentPageUrl, _apartComplexName);
                var apartComplexHRefs = ParseHtmlHRefs(currentPageUrl, _apartComplexHRef);

                var apartComplexData = apartComplexNames.Zip(apartComplexHRefs, (name, hRef) => new ApartComplex
                {
                    Name = name,
                    CityName = apartComplexGroupData.CityName,
                    Url = _homePageUrl + hRef
                });

                if(apartComplexData != null)
                    apartComplexDataPerCity.AddRange(apartComplexData);

                nextPageExists = ParseHtmlNextPageExists(apartComplexGroupData.Url + pageTag, _pageNumbers);
            }
            while (false);// nextPageExists);

            return apartComplexDataPerCity;
        }

        public ICollection<ApartComplex> ParseSpecificData()
        {
            var cityData = GetCityData();
            var apartComplexGroupData = GetApartComplexGroupData(cityData);

            var apartComplexes = GetApartComplexes(apartComplexGroupData);               
            SetApartments(ref apartComplexes);

            foreach (var apartComplex in apartComplexes)
            {
                Console.WriteLine(apartComplex.Name + " " + apartComplex.CityName + " " + apartComplex.Url);

                if (apartComplex.Apartments != null)
                {
                    foreach (var apartment in apartComplex.Apartments)
                        Console.WriteLine(apartment.NumberOfRooms + " " + apartment.DwellingSpaceMin + " " + apartment.DwellingSpaceMax + " " + apartment.SquareMeterPriceMin + " " + apartment.SquareMeterPriceMax);
                }
            }

            return apartComplexes;
        }


    }
}
