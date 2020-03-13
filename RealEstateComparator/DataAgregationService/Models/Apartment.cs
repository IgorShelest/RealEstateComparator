using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.Models
{
    public class Apartment
    {
        public int Id { get; set; }
                        
        public string NumberOfRooms { get; set; }

        public int DwellingSpaceMin { get; set; }

        public int DwellingSpaceMax { get; set; }

        public int SquareMeterPriceMin { get; set; }

        public int SquareMeterPriceMax { get; set; }

        public int ComplexId { get; set; }

        public ApartComplex Complex { get; set; }
    }
}
