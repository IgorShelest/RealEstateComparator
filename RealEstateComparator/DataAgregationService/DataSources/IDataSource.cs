using DataAgregationService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.DataSources
{
    interface IDataSource
    {        
        string GetUrl();
        string GetXPath();
    }
}
