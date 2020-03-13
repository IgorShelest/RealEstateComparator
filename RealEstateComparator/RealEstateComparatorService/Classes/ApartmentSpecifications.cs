using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateComparatorService.Classes
{
    public class ApartmentSpecifications
    {
        public string City { get; set; }

        public string NumberOfRooms { get; set; }

        public int DwellingSpace { get; set; }

        public int RenovationPricePerMeter { get; set; }

        public int OverallPrice { get; set; }
    }
}
