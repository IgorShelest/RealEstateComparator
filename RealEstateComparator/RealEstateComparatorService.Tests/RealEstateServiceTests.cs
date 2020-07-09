using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContextRepositories.Dto;
using ApplicationContexts.Models;
using Moq;
using RealEstateComparatorService.Services;
using Xunit;

namespace RealEstateComparatorTests
{
    public class RealEstateServiceTests
    {
        [Fact]
        public void GetBetterApartments()
        {
            // Arrange
            var apartmentSpecs = CreateApartmentSpecsDto();
            var expectedResult = CreateApartmentsByPriceSpecs();
            var apartmentsByPhysicalSpecs = CreateApartmentsByPhysicalSpecs(expectedResult);
            var apartmentRepositoryMock = MockApartmentRepository(apartmentSpecs, apartmentsByPhysicalSpecs);
            var realEstateService = new RealEstateService(apartmentRepositoryMock.Object, null);

            // Act
            var actualResult = realEstateService.GetBetterApartments(apartmentSpecs);
            
            // Assert
            Assert.Equal(expectedResult.Count(), actualResult.Count());
        }

        [Fact]
        public async Task GetApartComplex()
        {
            // Arrange
            var expectedResult = CreateApartComplex();
            const int apartComplexIndex = 7;
            var apartComplexRepositoryMock = MockApartComplexRepository(apartComplexIndex, expectedResult);
            var realEstateService = new RealEstateService(null, apartComplexRepositoryMock.Object);

            // Act
            var actualResult = await realEstateService.GetApartComplex(apartComplexIndex);
            
            // Assert
            Assert.Equal(expectedResult.Source, actualResult.Source);
            Assert.Equal(expectedResult.Name, actualResult.Name);
            Assert.Equal(expectedResult.CityName, actualResult.CityName);
            Assert.Equal(expectedResult.Url, actualResult.Url);
        }

        private Mock<ApartComplexRepository> MockApartComplexRepository(int apartComplexIndex, ApartComplex expectedResult)
        {
            var apartComplexRepositoryMock = new Mock<ApartComplexRepository>(null);
            apartComplexRepositoryMock
                .Setup(repository => repository.GetApartComplex(apartComplexIndex))
                .ReturnsAsync(expectedResult);

            return apartComplexRepositoryMock;
        }

        private ApartComplex CreateApartComplex()
        {
            return new ApartComplex
            {
                Source = "SourceName",
                Name = "ApartComplexName",
                CityName = "CityName",
                Url = "Url"
            };
        }

        private Mock<ApartmentRepository> MockApartmentRepository(ApartmentSpecsDto apartmentSpecsDto, IEnumerable<Apartment> apartmentsByPhysicalSpecs)
        {
            var apartmentRepositoryMock = new Mock<ApartmentRepository>(null);
            apartmentRepositoryMock
                .Setup(repository => repository.GetApartments(apartmentSpecsDto))
                .Returns(apartmentsByPhysicalSpecs);

            return apartmentRepositoryMock;
        }

        private ApartmentSpecsDto CreateApartmentSpecsDto()
        {
            return new ApartmentSpecsDto()
            {
                City = "Львів",
                NumberOfRooms = 2,
                HasMultipleFloors = false,
                DwellingSpace = 65,
                RenovationPricePerMeter = 12000,
                OverallPrice = 10000000
            };
        }

        private IEnumerable<Apartment> CreateApartmentsByPriceSpecs()
        {
            return new List<Apartment>
            {
                new Apartment()
                {
                    NumberOfRooms = 2,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 59,
                    DwellingSpaceMax = 82,
                    SquareMeterPriceMin = 17800,
                    SquareMeterPriceMax = 22400
                },
                new Apartment()
                {
                    NumberOfRooms = 2,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 57,
                    DwellingSpaceMax = 65,
                    SquareMeterPriceMin = 15000,
                    SquareMeterPriceMax = 18400
                }
            };
        }

        private IEnumerable<Apartment> CreateApartmentsByPhysicalSpecs(IEnumerable<Apartment> apartmentsByPriceSpecs)
        {
            return apartmentsByPriceSpecs
                .Append(new Apartment()
                {
                    NumberOfRooms = 2,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 60,
                    DwellingSpaceMax = 70,
                    SquareMeterPriceMin = 150500,
                    SquareMeterPriceMax = 160600
                })
                .Append(new Apartment()
                {
                    NumberOfRooms = 2,
                    HasMultipleFloors = false,
                    DwellingSpaceMin = 60,
                    DwellingSpaceMax = 70,
                    SquareMeterPriceMin = 160600,
                    SquareMeterPriceMax = 170700
                });
        }
    }
}