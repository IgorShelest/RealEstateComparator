using DataAgregationService.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.DataSources
{
    class LunUa : IDataSource
    {
        private readonly string siteUrl = "https://lun.ua/";
        private readonly string cityXPath = "/html/body/div[3]/div[2]/div[1]/a[@data-analytics-click='geo_list|goto_catalog']";
        
        public string GetUrl()
        {
            return siteUrl;
        }

        public string GetXPath()
        {
            return cityXPath;
        }
    }
}
