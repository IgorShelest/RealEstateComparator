using System.Collections.Generic;

namespace ApplicationContexts.Models
{
    public class ApartComplex
    {
        public int Id { get; set; }

        public string Source { get; set; }

        public string Name { get; set; }

        public string CityName { get; set; }

        public string Url { get; set; }

        public IEnumerable<Apartment> Apartments { get; set; }
    }
}
