using Microsoft.Extensions.Configuration;

namespace DataAggregationService.Services
{
    public class ConfigHandler : IConfigHandler
    {
        private readonly IConfigurationRoot _configRoot;
        
        public ConfigHandler(string configFileName)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile(configFileName);
            _configRoot = configBuilder.Build();
        }

        public string GetConnectionString(string connectionStringName)
        {
            return _configRoot.GetConnectionString(connectionStringName);
        }
    }
}