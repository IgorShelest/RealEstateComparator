namespace ApplicationContextRepositories.Dto
{
    public class ApartmentDto
    {
        public int NumberOfRooms { get; set; }
                        
        public bool HasMultipleFloors { get; set; }

        public int DwellingSpaceMin { get; set; }

        public int DwellingSpaceMax { get; set; }

        public int SquareMeterPriceMin { get; set; }

        public int SquareMeterPriceMax { get; set; }
        
        public int ComplexId { get; set; }
    }
}