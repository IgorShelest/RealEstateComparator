using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.Models
{
    class ApartComplex: LunUaData
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CityName { get; set; }

        public string Url { get; set; }

        public IEnumerable<Apartment> Apartments { get; set; }
    }
}
