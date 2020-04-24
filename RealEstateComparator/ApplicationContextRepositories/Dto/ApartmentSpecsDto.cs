namespace ApplicationContextRepositories.Dto
{
    public class ApartmentSpecsDto
    {
        public string City { get; set; }

        public int NumberOfRooms { get; set; }
                        
        public bool HasMultipleFloors { get; set; }

        public int DwellingSpace { get; set; }

        public int RenovationPricePerMeter { get; set; }

        public int OverallPrice { get; set; }
    }
}
