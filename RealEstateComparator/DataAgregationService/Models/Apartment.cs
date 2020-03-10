using System;
using System.Collections.Generic;
using System.Text;

namespace DataAgregationService.Models
{
    class Apartment: LunUaData
    {
        public int Id { get; set; }
                        
        public string NumberOfRooms { get; set; }

        public int DwellingSpaceMin { get; set; }

        public int DwellingSpaceMax { get; set; }

        public decimal SquareMeterPriceMin { get; set; }

        public decimal SquareMeterPriceMax { get; set; }

        public int ComplexId { get; set; }

        public ApartComplex Complex { get; set; }
    }
}
