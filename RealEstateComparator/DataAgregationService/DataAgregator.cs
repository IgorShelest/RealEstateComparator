using DataAgregationService.DataSources;
using DataAgregationService.Models;
using DataAgregationService.Parsers;
using System.Collections.Generic;

namespace DataAgregationService
{
    class DataAgregator
    {
        // Db data
        private List<ApartComplex> _apartComplexes;

        // Internal data
        private List<IDataSource> _dataSources;

        public DataAgregator( )
        {
            _apartComplexes = new List<ApartComplex>();

            _dataSources = new List<IDataSource>() {new LunUa()};
        }

        public void Run()
        {
            GetData();
            UpdateDb();
        }

        private void GetData()
        {
            var apartComplexParser = new LunUaParser();
            var apartComplexes = apartComplexParser.ParseSpecificData();
            _apartComplexes.AddRange(apartComplexes);
        }

        private void UpdateDb()
        {
        }
    }
}
