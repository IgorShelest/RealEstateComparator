using DataAggregationService.Services;
using Xunit;

namespace DataAggregationService.Tests
{
    public class ConfigHandlerTests
    {
        private const string configFile = "appsettings.json";
        
        [Fact]
        public void GetConnectionString_StringReturned()
        {
            // Arrange
            const string connectionStringName = "DefaultConnection";
            var configHandler = new ConfigHandler(configFile);
            
            // Act
            var connectionString = configHandler.GetConnectionString(connectionStringName);

            // Assert
            Assert.NotEmpty(connectionString);
        }
        
        [Fact]
        public void GetConnectionString_NullReturned()
        {
            // Arrange
            const string connectionStringName = "InvalidConnection";
            var configHandler = new ConfigHandler(configFile);
            
            // Act
            var connectionString = configHandler.GetConnectionString(connectionStringName);

            // Assert
            Assert.Null(connectionString);
        }
    }
}