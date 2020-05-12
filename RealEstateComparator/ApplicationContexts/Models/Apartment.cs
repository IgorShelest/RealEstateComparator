namespace ApplicationContexts.Models
{
    public class Apartment
    {
        public int Id { get; set; }
                        
        public int NumberOfRooms { get; set; }
                        
        public bool HasMultipleFloors { get; set; }

        public int DwellingSpaceMin { get; set; }

        public int DwellingSpaceMax { get; set; }

        public int SquareMeterPriceMin { get; set; }

        public int SquareMeterPriceMax { get; set; }

        public ApartComplex Complex { get; set; }
    }
}
