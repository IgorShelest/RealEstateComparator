using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationContextRepositories;
using ApplicationContexts;
using ApplicationContexts.Models;
using DataAggregationService.Aggregators.DomRia;
using DataAggregationService.Aggregators.LunUa;
using DataAggregationService.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace DataAggregationService.Tests
{
    public class DataAggregatorTests
    {
        [Fact]
        public async Task Run()
        {
            // Arrange
            var apartComplexRepositoryMock = MockApartComplexRepository();
            var lunUaApartComplexes = CreateRandomApartComplexes();
            var lunUaAggregator = MockLunUaAggregator(lunUaApartComplexes);
            var domRiaApartComplexes = CreateRandomApartComplexes();
            var domRiaAggregator = MockDomRiaAggregator(domRiaApartComplexes);
            var expectedResult = CreateExpectedResult(lunUaApartComplexes, domRiaApartComplexes).ToList();
            var aggregators = new List<IAggregator>
            {
                lunUaAggregator.Object,
                domRiaAggregator.Object
            };
            var dataAggregator = new DataAggregator(apartComplexRepositoryMock, aggregators);
            
            // Act
            await dataAggregator.Run();
            
            // Assert
            Assert.Equal(expectedResult.Count(), ((TestRepository)apartComplexRepositoryMock)._apartComplexes.Count());

            for (var iter = 0; iter < expectedResult.Count(); iter++)
                Assert.True(CompareApartComplexes(expectedResult[iter], ((TestRepository)apartComplexRepositoryMock)._apartComplexes.ToList()[iter]));
        }

        private IApartComplexRepository MockApartComplexRepository()
        {
            return new TestRepository();
        }

        class TestRepository : IApartComplexRepository
        {
            public IEnumerable<ApartComplex> _apartComplexes;
        
            public async Task UpdateDb(IEnumerable<ApartComplex> apartComplexes)
            {
                _apartComplexes = apartComplexes;
            }

            public Task<ApartComplex> GetApartComplex(int ComplexId)
            {
                throw new NotImplementedException();
            }
        }

        private bool CompareApartComplexes(ApartComplex lhs, ApartComplex rhs)
        {
            return lhs.Source == rhs.Source
                   && lhs.Name == rhs.Name
                   && lhs.CityName == rhs.CityName
                   && lhs.Url == rhs.Url;
        }

        private IEnumerable<ApartComplex> CreateExpectedResult(IEnumerable<ApartComplex> lunUaApartComplexes,
            IEnumerable<ApartComplex> domRiaApartComplexes)
        {
            var expectedResult = new List<ApartComplex>();
            expectedResult.AddRange(lunUaApartComplexes);
            expectedResult.AddRange(domRiaApartComplexes);
            return expectedResult;
        }

        private IEnumerable<ApartComplex> CreateRandomApartComplexes()
        {
            const int numberOfApartComplexes = 4;
            var apartments = new List<ApartComplex>();
            var random = new Random();
            
            for (var iter = 0; iter < numberOfApartComplexes; iter++)
            {
                apartments.Add(new ApartComplex()
                {
                    Source = $"Source_{random.Next()}",
                    Name = $"Name{random.Next()}",
                    CityName = $"CityName{random.Next()}",
                    Url = $"Url{random.Next()}",
                    Apartments = CreateRandomApartments()
                });
            }

            return apartments;        }

        private IEnumerable<Apartment> CreateRandomApartments()
        {
            const int numberOfApartments = 4;
            var apartments = new List<Apartment>();
            var random = new Random();
            
            for (var iter = 0; iter < numberOfApartments; iter++)
            {
                apartments.Add(new Apartment()
                {
                    NumberOfRooms = random.Next(),
                    HasMultipleFloors = Convert.ToBoolean(random.Next(2)),
                    DwellingSpaceMin = random.Next(),
                    DwellingSpaceMax = random.Next(),
                    SquareMeterPriceMin = random.Next(),
                    SquareMeterPriceMax = random.Next()
                });
            }

            return apartments;
        }

        private Mock<LunUaAggregator> MockLunUaAggregator(IEnumerable<ApartComplex> apartComplexes)
        {
            var lunUaAggregator = new Mock<LunUaAggregator>(null, null, null);
            lunUaAggregator
                .Setup(aggregator => aggregator.AggregateData())
                .ReturnsAsync(apartComplexes);

            return lunUaAggregator;
        }

        private Mock<DomRiaAggregator> MockDomRiaAggregator(IEnumerable<ApartComplex> apartComplexes)
        {
            var domRiaAggregator = new Mock<DomRiaAggregator>(null, null);
            domRiaAggregator
                .Setup(aggregator => aggregator.AggregateData())
                .ReturnsAsync(apartComplexes);

            return domRiaAggregator;
        }
    }
}